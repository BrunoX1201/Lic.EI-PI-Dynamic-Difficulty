from pydantic import BaseModel


class ProcessOptions(BaseModel):
    rollback_on_success: bool = False


# Modelo de dados para o pedido do Unity
class ProcessEncounterRequest(BaseModel):
    encounter_id: str
    options: ProcessOptions = ProcessOptions()
