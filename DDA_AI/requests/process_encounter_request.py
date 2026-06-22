from pydantic import BaseModel


# Modelo de dados para o pedido do Unity
class ProcessEncounterRequest(BaseModel):
    encounter_id: str
