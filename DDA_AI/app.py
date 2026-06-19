from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import aggregation_attributes as ag
from steps import PreProcessingStep

app = FastAPI(title="Unity DDA Data Pipeline API")

EVENT_BASE_PATH = "./testdata/"
OUTPUT_PATH = "./output"
OUTPUT_FILE = "test"

# Modelo de dados para o pedido do Unity
class EncounterRequest(BaseModel):
    encounter_id: str

def build_pre_processing_step() -> PreProcessingStep:
    attributes = [
        ag.AverageTimeBetweenKillsAttribute(),
        ag.EncounterTotalTimeAttribute(),
        ag.HasCompletedEncounterAttribute(),
        ag.PlayerAverageAccuracyAttribute(),
        ag.RemainingEnemiesAttribute(),
        ag.RemainingPlayerHealthAttribute(),
        ag.TotalHitsTakenAttribute(),
    ]
    return PreProcessingStep(EVENT_BASE_PATH, OUTPUT_PATH, OUTPUT_FILE, attributes)

def to_native(value):
    if hasattr(value, "item"):
        return value.item()
    return value

@app.post("/api/process_encounter")
def process_encounter(request: EncounterRequest):
    encounter_id = request.encounter_id
    pre_process = build_pre_processing_step()
    
    try:
        # 1. Executa a transformação com o ID enviado pelo Unity
        output_data = pre_process.transform(encounter_id)
        
        # Se a lista voltar vazia, significa que o ID não foi encontrado nos CSVs
        if not output_data:
            raise HTTPException(
                status_code=404, 
                detail=f"Encounter ID '{encounter_id}' não foi encontrado ou falhou nos limites."
            )
        
        # 2. Guarda o output no CSV local
        pre_process.save_output()
        
        # 3. Transforma a lista de tuplos [("nome", valor), ...] num dicionário para o JSON
        metrics_dict = {attr_name: to_native(attr_value) for attr_name, attr_value in output_data}
        
        return {
            "status": "success",
            "encounter_id": encounter_id,
            "metrics": metrics_dict
        }
        
    except HTTPException as http_ex:
        raise http_ex
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro interno no processamento: {str(e)}")

# Para correr o servidor diretamente pelo Python
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)