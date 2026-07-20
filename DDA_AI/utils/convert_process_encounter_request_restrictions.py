from requests.process_encounter.next_encounter_restrictions import NextEncounterRestrictions
from shared.mobile_npc_restriction import MobileNPCRestriction
from shared.observation_restriction import ObservationRestriction
from shared.restriction import Restriction
from shared.turret_npc_restriction import TurretNPCRestriction


def convert_process_encounter_request_restrictions(restrictions: NextEncounterRestrictions) -> dict[
    str, ObservationRestriction]:
    turret_restriction = TurretNPCRestriction(min_count=restrictions.turret.count.min_limit,
                                              max_count=restrictions.turret.count.max_limit,
                                              default_count=restrictions.turret.count.default_value,
                                              previous_count=restrictions.turret.count.previous_value,

                                              min_hp=restrictions.turret.health.min_limit,
                                              max_hp=restrictions.turret.health.max_limit,
                                              default_hp=restrictions.turret.health.default_value,
                                              previous_hp=restrictions.turret.health.previous_value,

                                              min_hitbox=restrictions.turret.hitbox.min_limit,
                                              max_hitbox=restrictions.turret.hitbox.max_limit,
                                              default_hitbox=restrictions.turret.hitbox.default_value,
                                              previous_hitbox=restrictions.turret.hitbox.previous_value)

    mobile_restriction = MobileNPCRestriction(min_count=restrictions.mobile.count.min_limit,
                                              max_count=restrictions.mobile.count.max_limit,
                                              default_count=restrictions.mobile.count.default_value,
                                              previous_count=restrictions.mobile.count.previous_value,

                                              min_hp=restrictions.mobile.health.min_limit,
                                              max_hp=restrictions.mobile.health.max_limit,
                                              default_hp=restrictions.mobile.health.default_value,
                                              previous_hp=restrictions.mobile.health.previous_value,

                                              min_hitbox=restrictions.mobile.hitbox.min_limit,
                                              max_hitbox=restrictions.mobile.hitbox.max_limit,
                                              default_hitbox=restrictions.mobile.hitbox.default_value,
                                              previous_hitbox=restrictions.mobile.hitbox.previous_value)

    return {
        Restriction.TURRET_NPC: turret_restriction,
        Restriction.MOBILE_NPC: mobile_restriction
    }
