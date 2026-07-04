import datetime
import math
from pathlib import Path

from fastapi import FastAPI, HTTPException

import aggregation_attributes as ag
from requests import ProcessEncounterRequest
from steps import PreProcessingStep

app = FastAPI(title="Unity DDA Data Pipeline API")

EVENT_BASE_PATH = Path("./data/test/")
OUTPUT_PATH = Path("./output/runtime")
OUTPUT_FILE = "pre_processing"

attributes = [
    ag.AverageTimeBetweenKillsAttribute(),
    ag.EncounterTotalTimeAttribute(),
    ag.HasCompletedEncounterAttribute(),
    ag.PlayerAverageAccuracyAttribute(),
    ag.RemainingEnemiesAttribute(),
    ag.RemainingPlayerHealthAttribute(),
    ag.TotalHitsTakenAttribute(),
]

pre_process = PreProcessingStep(str(EVENT_BASE_PATH), str(OUTPUT_PATH), OUTPUT_FILE, attributes)
pre_process_last_save_path = ""


def numpy_to_native(value):
    if hasattr(value, "item"):
        return value.item()
    return value

@app.post("/api/process_encounter/test")
def process_encounter_test(request: ProcessEncounterRequest):
    try:
        return {
            "status": "success",
            "agent": "K_means",
            "action": "<ação tomada pelo agente>",
            "action_params": {
                "total_enemies": 2,
                "enemy_health": 0.25,
                "enemy_hitbox": 1.1
            }
        }
    except Exception as e:
        print(f"[ERROR] {e}")
        raise HTTPException(status_code=500, detail=f"Erro interno no processamento: {str(e)}")


@app.post("/api/process_encounter")
def process_encounter(request: ProcessEncounterRequest):
    global pre_process_last_save_path
    encounter_id = request.encounter_id

    try:
        # 1. Executa a transformação com o ID enviado pelo Unity
        output_data = pre_process.execute(encounter_id)

        # Se a lista voltar vazia, significa que o ID não foi encontrado nos CSVs
        if not output_data:
            raise HTTPException(
                status_code=404,
                detail=f"Encounter ID '{encounter_id}' não foi encontrado ou falhou nos limites."
            )

        # 2. Guarda o output no CSV local
        executed_time = datetime.datetime.now(datetime.timezone.utc)
        pre_process_last_save_path = pre_process.save_output({"processed_at": math.floor(executed_time.timestamp())},
                                                             pre_process_last_save_path)

        # 3. Transforma a lista de tuplos [("nome", valor), ...] num dicionário para o JSON
        metrics_dict = {attr_name: numpy_to_native(attr_value) for attr_name, attr_value in output_data}

        return {
            "status": "success",
            "agent": "<nome do agente usado>",
            "action": "<ação tomada pelo agente>",
            "action_params": {
                "total_enemies": 2,
                "enemy_health": 0.25,
                "enemy_hitbox": 1
            }
        }

    except Exception as e:
        print(f"[ERROR] {e}")
        raise HTTPException(status_code=500, detail=f"Erro interno no processamento: {str(e)}")

# Para correr o servidor diretamente pelo Python
if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="127.0.0.1", port=8000)

