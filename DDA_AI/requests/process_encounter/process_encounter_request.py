from pydantic import BaseModel

from .next_encounter_restrictions import NextEncounterRestrictions
from .process_options import ProcessOptions


# Modelo de dados para o pedido do Unity
class ProcessEncounterRequest(BaseModel):
    encounter_id: str
    next_encounter_restrictions: NextEncounterRestrictions
    options: ProcessOptions = ProcessOptions()
