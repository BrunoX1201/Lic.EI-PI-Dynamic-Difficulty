from shared.npc_restriction import NPCRestriction


class TurretNPCRestriction(NPCRestriction):

    def __init__(self,
                 min_count: int = 0, max_count: int = 3, default_count: int = 0, previous_count: int = 0,
                 min_hp=1.0, max_hp: float = 300.0, default_hp: float = 100.0, previous_hp: float = 100.0,
                 min_hitbox: float = 0.8, max_hitbox: float = 1.25, default_hitbox: float = 1.0,
                 previous_hitbox: float = 1.0) -> None:
        self.min_count = min_count
        self.max_count = max_count
        self.default_count = default_count
        self.previous_count = previous_count

        self.min_hp = min_hp
        self.max_hp = max_hp
        self.default_hp = default_hp
        self.previous_hp = previous_hp

        self.min_hitbox = min_hitbox
        self.max_hitbox = max_hitbox
        self.default_hitbox = default_hitbox
        self.previous_hitbox = previous_hitbox
