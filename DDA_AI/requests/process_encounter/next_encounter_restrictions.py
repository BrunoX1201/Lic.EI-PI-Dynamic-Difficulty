from pydantic import BaseModel


class RestrictionValueRange(BaseModel):
    min_limit: int | float
    max_limit: int | float
    default_value: int | float
    previous_value: int | float


class NPCRestriction(BaseModel):
    count: RestrictionValueRange
    health: RestrictionValueRange
    hitbox: RestrictionValueRange


class NextEncounterRestrictions(BaseModel):
    turret: NPCRestriction
    mobile: NPCRestriction
