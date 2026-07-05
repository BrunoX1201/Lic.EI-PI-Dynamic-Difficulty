import datetime
import math

from fastapi import APIRouter, HTTPException

from dependencies import session_dependency, dda_pipeline_dependency
from requests import ProcessEncounterRequest
from utils import numpy_to_native

__DATA_PREPARATION_OUTPUT_FILE = "data_preparation"

router = APIRouter()


@router.post("/process_encounter")
def process_encounter(request: ProcessEncounterRequest, session_service: session_dependency,
                      dda_pipeline_service: dda_pipeline_dependency):
    if not session_service.is_configured:
        msg = "The session has not yet been configured."
        print(f"[ERROR] {msg}")
        raise HTTPException(status_code=409, detail=msg)

    encounter_id = request.encounter_id

    try:
        # 1. Executa a transformação com o ID enviado pelo Unity
        output_data = dda_pipeline_service.data_preparation.execute(encounter_id, request.options.rollback_on_success)

        # Se a lista voltar vazia, significa que o ID não foi encontrado nos CSVs
        if not output_data:
            raise HTTPException(
                status_code=404,
                detail=f"Encounter ID '{encounter_id}' não foi encontrado ou falhou nos limites."
            )

        # 2. Guarda o output no CSV local
        executed_time = datetime.datetime.now(datetime.timezone.utc)
        dda_pipeline_service.data_preparation.save_output(session_service.output_path, __DATA_PREPARATION_OUTPUT_FILE,
                                                          {"processed_at": math.floor(executed_time.timestamp())})

        # 3. Transforma a lista de tuplos [("nome", valor), ...] num dicionário para o JSON
        metrics_dict = {attr_name: numpy_to_native(attr_value) for attr_name, attr_value in output_data}

        return {
            "status": "success",
            "encounter_id": encounter_id,
            "metrics": metrics_dict
        }

    except Exception as e:
        print(f"[ERROR] {e}")
        raise HTTPException(status_code=500, detail=f"Erro interno no processamento: {str(e)}")
