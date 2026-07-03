import datetime
import math
from pathlib import Path

from fastapi import FastAPI, HTTPException

import aggregation_attributes as ag
from requests import ProcessEncounterRequest
from steps import DataPreparationStep

app = FastAPI(title="Unity DDA Data Pipeline API")

EVENT_BASE_PATH = Path("./data/test/")
OUTPUT_PATH = Path("./output/runtime")
OUTPUT_FILE = "data_preparation"

attributes = [
    ag.AverageTimeBetweenKillsAttribute(),
    ag.EncounterTotalTimeAttribute(),
    ag.HasCompletedEncounterAttribute(),
    ag.PlayerAverageAccuracyAttribute(),
    ag.RemainingEnemiesAttribute(),
    ag.RemainingPlayerHealthAttribute(),
    ag.TotalHitsTakenAttribute(),
]

data_preparation = DataPreparationStep(str(EVENT_BASE_PATH), str(OUTPUT_PATH), OUTPUT_FILE, attributes)
data_preparation_last_save = ""


def numpy_to_native(value):
    if hasattr(value, "item"):
        return value.item()
    return value


@app.post("/api/process_encounter")
def process_encounter(request: ProcessEncounterRequest):
    global data_preparation_last_save
    encounter_id = request.encounter_id

    try:
        # 1. Executa a transformação com o ID enviado pelo Unity
        output_data = data_preparation.execute(encounter_id)

        # Se a lista voltar vazia, significa que o ID não foi encontrado nos CSVs
        if not output_data:
            raise HTTPException(
                status_code=404,
                detail=f"Encounter ID '{encounter_id}' não foi encontrado ou falhou nos limites."
            )

        # 2. Guarda o output no CSV local
        executed_time = datetime.datetime.now(datetime.timezone.utc)
        data_preparation_last_save = data_preparation.save_output(
            {"processed_at": math.floor(executed_time.timestamp())},
            data_preparation_last_save)

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


# Para correr o servidor diretamente pelo Python
if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="127.0.0.1", port=8000)
