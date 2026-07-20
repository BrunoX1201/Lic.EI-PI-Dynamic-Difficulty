from numpy import dtype

from shared import Event

__shared_attributes_map: dict[str, dtype] = {
    "session_id": dtype(str),
    "event_type": dtype(str),
    "timestamp": dtype(str),
}
__encounter_shared_attributes_map: dict[str, dtype] = {"encounter_id": dtype(str)}

event_attributes_map: dict[Event, dict[str, dtype]] = {
    Event.ENCOUNTER_STARTED:
        __shared_attributes_map |
        __encounter_shared_attributes_map |
        {
            "start_reason": dtype(str),
            "total_enemies": dtype(int),
            "player_start_health": dtype(float),
            "player_start_ammo": dtype(float),
            "location": dtype(str),
        },
    Event.ENCOUNTER_ENDED:
        __shared_attributes_map |
        __encounter_shared_attributes_map |
        {
            "end_reason": dtype(str),
            "player_remaining_health": dtype(float),
            "player_remaining_ammo": dtype(float),
            "remaining_enemies": dtype(int),
        },
    Event.ENCOUNTER_ZONE_ENTERED:
        __shared_attributes_map |
        __encounter_shared_attributes_map |
        {
            "player_position": dtype(str)
        },
    Event.ENCOUNTER_ZONE_LEFT:
        __shared_attributes_map |
        __encounter_shared_attributes_map |
        {
            "player_position": dtype(str)
        },
    Event.GAME_TIME_PASSED:
        __shared_attributes_map |
        {
            "total_game_time_seconds": dtype(int),
            "player_location": dtype(str)
        },
    Event.ITEM_PICKED_UP:
        __shared_attributes_map |
        {
            "player_id": dtype(int),
            "item": dtype(str),
            "remaining_ammo": dtype(float),
            "remaining_health": dtype(float),
            "player_position": dtype(str)
        },
    Event.NEW_LOCATION_DISCOVERED:
        __shared_attributes_map |
        {
            "last_location": dtype(str),
            "new_location": dtype(str),
            "time_in_last_location_seconds": dtype(int),
            "has_discovered": dtype(bool)
        },
    Event.PLAYER_ATTACKED:
        __shared_attributes_map |
        {
            "player_id": dtype(int),
            "target_id": dtype(int),
            "is_hit": dtype(bool),
            "accuracy": dtype(float),
            "player_position": dtype(str),
            "weapon_used": dtype(str),
            "damage_per_hit": dtype(float),
            "attack_type": dtype(str),
            "remaining_ammo": dtype(float)
        },
    Event.PLAYER_DIED:
        __shared_attributes_map |
        {
            "killed_by": dtype(int),
            "death_count": dtype(int),
            "time_alive_seconds": dtype(int),
            "player_position": dtype(str)
        },
    Event.PLAYER_TOOK_DAMAGE:
        __shared_attributes_map |
        {
            "damage_taken_per_hit": dtype(float),
            "player_move_speed": dtype(float),
            "instigator": dtype(int),
            "player_current_health": dtype(float),
            "player_position": dtype(str)
        },
    Event.PLAYER_WENT_AIRBORNE:
        __shared_attributes_map |
        {
            "start_position": dtype(str),
            "end_position": dtype(str),
            "action": dtype(str)
        },
    Event.TARGET_KILLED:
        __shared_attributes_map |
        {
            "target_id": dtype(int),
            "weapon_used": dtype(str),
            "remaining_ammo": dtype(float),
            "remaining_health": dtype(float),
            "player_position": dtype(str),
            "player_location": dtype(str)
        }
}
