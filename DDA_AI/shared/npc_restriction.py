from .observation_restriction import ObservationRestriction


class NPCRestriction(ObservationRestriction):
    min_count: int
    max_count: int
    default_count: int
    previous_count: int

    min_hp: float
    max_hp: float
    default_hp: float
    previous_hp: float

    min_hitbox: float
    max_hitbox: float
    default_hitbox: float
    previous_hitbox: float

    def __str__(self) -> str:
        return (f"{{"
                f"min_count: {self.min_count}, "
                f"max_count: {self.max_count}, "
                f"default_count: {self.default_count}, "
                f"previous_count: {self.previous_count}, "

                f"min_hp: {self.min_hp}, "
                f"max_hp: {self.max_hp}, "
                f"default_hp: {self.default_hp}, "
                f"previous_hp: {self.previous_hp}, "

                f"min_hitbox: {self.min_hitbox}, "
                f"max_hitbox: {self.max_hitbox}, "
                f"default_hitbox: {self.default_hitbox}, "
                f"previous_hitbox: {self.previous_hitbox}"
                f"}}")
