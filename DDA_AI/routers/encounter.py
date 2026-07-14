import datetime
import math

from fastapi import APIRouter, HTTPException

from config import config
from dependencies import session_dependency, dda_pipeline_dependency
from requests import ProcessEncounterRequest
from utils import convert_process_encounter_request_restrictions, numpy_to_native

__DATA_PREPARATION_OUTPUT_FILE = config["DATA_PREPARATION_STEP_OUTPUT_FILENAME"]
__DECISIONS_OUTPUT_FILE = config["DECISION_MAKER_STEP_OUTPUT_FILENAME"]
__RESPONSE_ACTION_PARAMS_ROUNDINGS = {
    "count": 0,
    "health": 1,
    "hitbox": 2,
}

router = APIRouter()


@router.post("/process_encounter")
def process_encounter(request: ProcessEncounterRequest, session_service: session_dependency,
                      dda_pipeline_service: dda_pipeline_dependency):
    encounter_id = request.encounter_id

    try:
        # 1. Executa a transformação com o ID enviado pelo Unity
        data_prep_output = dda_pipeline_service.data_preparation.execute(encounter_id,
                                                                         request.options.rollback_on_success)
        encounter_start = dda_pipeline_service.data_preparation.encounter_start
        encounter_end = dda_pipeline_service.data_preparation.encounter_end

        # Se a lista voltar vazia, significa que o ID não foi encontrado nos CSVs
        if not data_prep_output:
            raise HTTPException(
                status_code=404,
                detail=f"Encounter ID '{encounter_id}' não foi encontrado ou falhou nos limites."
            )

        # 2. Guarda o output no CSV local
        executed_time = datetime.datetime.now(datetime.timezone.utc)
        dda_pipeline_service.data_preparation.save_output(session_service.output_path, __DATA_PREPARATION_OUTPUT_FILE,
                                                          {"processed_at": math.floor(executed_time.timestamp())})

        full_observation = {key: val for key, val in data_prep_output}
        converted_restrictions = convert_process_encounter_request_restrictions(request.next_encounter_restrictions)
        full_observation.update({"restrictions": converted_restrictions})

        decision_maker_output = dda_pipeline_service.decision_maker.decide(full_observation)
        # decision-maker and action name
        extra_info = [decision_maker_output.pop(0), decision_maker_output.pop(0)]
        instigator = extra_info[0][1]
        action_name = extra_info[1][1]

        executed_time = datetime.datetime.now(datetime.timezone.utc)
        dda_pipeline_service.decision_maker.save_output(session_service.output_path, __DECISIONS_OUTPUT_FILE,
                                                        {
                                                            "processed_at": math.floor(executed_time.timestamp()),
                                                            "decision_maker": instigator,
                                                            "action_name": action_name,
                                                            "session_id": encounter_start["session_id"],
                                                            "encounter_id": encounter_id,
                                                            "encounter_start_timestamp": encounter_start["timestamp"],
                                                            "encounter_end_timestamp": encounter_end["timestamp"],
                                                        })

        params = {"turret": {"count": 0.0, "health": 0.0, "hitbox": 0.0},
                  "mobile": {"count": 0.0, "health": 0.0, "hitbox": 0.0}}

        i = 0
        for key in params.keys():
            for sub_key in params[key].keys():
                curr_val = decision_maker_output[i][1]
                next_val = decision_maker_output[i + 1][1]

                if curr_val != 0:
                    params[key][sub_key] = curr_val
                elif next_val != 0:
                    params[key][sub_key] = -next_val
                else:
                    params[key][sub_key] = 0.0

                i += 2

        for key in params.keys():
            for sub_key, value in params[key].items():
                native_val = numpy_to_native(value)
                params[key][sub_key] = native_val if not isinstance(native_val, float) else round(native_val,
                                                                                                  __RESPONSE_ACTION_PARAMS_ROUNDINGS[
                                                                                                      sub_key])

        return {
            "status": "success",
            "agent": dda_pipeline_service.decision_maker.selected_model.name,
            "action": action_name,
            "action_params": params,
        }

    except Exception as e:
        print(f"[ERROR] {e}")
        raise HTTPException(status_code=500, detail=f"Erro interno no processamento: {str(e)}")
