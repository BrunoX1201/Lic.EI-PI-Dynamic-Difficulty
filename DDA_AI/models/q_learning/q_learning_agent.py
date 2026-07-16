import json
from math import sqrt
from pathlib import Path
from random import randint, random
from typing import cast

import numpy as np
from numpy import typing as npt
from pandas import DataFrame

from shared import ObservationRestriction, Restriction, TurretNPCRestriction, MobileNPCRestriction, ActionDirection
from utils import calculate_additive_offset
from .action import Action
from .state import State
from ..dda_model import DDAModel


class QLearningAgent(DDAModel):
    # Discount factor for future rewards
    __gamma_factor: float = 0.9

    __learning_rate: float = 0.1

    __eps_start: float = 1
    __eps_curr: float
    __eps_decay: float = 0.995
    __eps_min: float = 0.05

    __previous_state: State | None = None
    __previous_action: Action | None = None

    __q_table: npt.NDArray[np.float64] = np.zeros((len(State), len(Action)))

    __score_weights: dict[str, float] = {
        "has_completed_encounter": 5.0,  # +
        "average_time_between_kills_seconds": 0.5,  # -
        "encounter_total_time_seconds": 0.6,  # -
        "player_average_accuracy": 0.3,  # +
        "remaining_enemies": 0.8,  # -
        "remaining_player_health": 0.9,  # +
        "total_hits_taken": 0.4,  # -
    }
    __min_score: float
    __max_score: float
    __score_intervals: dict[State, tuple[float, float]] = {}  # state: (min_limit, max_limit)

    __base_rewards: dict[State, float] = {
        State.OVERWHELMED: -1.0,
        State.STRUGGLING: -0.5,
        State.BALANCED: 1.0,
        State.COMFORTABLE: -0.5,
        State.DOMINATING: -1.0
    }
    __directional_bonus_reward: float = 0.2

    # Multipliers same as K Means Model
    __base_actions_map: dict[Action, npt.NDArray[np.float32]] = {
        Action.MUCH_EASIER: np.array([0.7, 0.7, 1.20], dtype=np.float32),
        Action.SLIGHTLY_EASIER: np.array([0.9, 0.9, 1.15], dtype=np.float32),
        Action.KEEP_SAME: np.array([1, 1, 1], dtype=np.float32),
        Action.SLIGHTLY_HARDER: np.array([1.1, 1.2, 0.90], dtype=np.float32),
        Action.MUCH_HARDER: np.array([1.3, 1.4, 0.85], dtype=np.float32)
    }

    @property
    def name(self) -> str:
        return "q-learning"

    @property
    def extension(self) -> str:
        return "json"

    def __init__(self) -> None:
        self.__eps_curr = self.__eps_start

        self.__calculate_min_score()
        self.__calculate_max_score()
        self.__calculate_score_intervals()

    def act(self, observation: dict[str, str | int | float | ObservationRestriction]) -> tuple[
        str, ActionDirection, npt.NDArray[np.float32]]:

        cp_observation = observation.copy()
        restrictions = cp_observation.pop("restrictions", None)
        if restrictions is None:
            raise KeyError(f"{self.name}: observation is incomplete")

        print(f"[ACTION] {self.name}, stats for obs {str(observation)}:")

        curr_score = self.__calculate_score(cp_observation)
        curr_state = self.__calculate_state(curr_score)
        print(f"- score: {curr_score}, new state: {curr_state.name}")

        if curr_state is None:
            raise ValueError(
                f"Invalid state, got score {curr_score} but limits are ({self.__min_score, self.__max_score})")

        # If its first observation
        chosen_action = None
        if self.__previous_state is None or self.__previous_action is None:
            print(f"- no reward since its first observation (previous_state or previous_action is None)")
            print(f"[ACTION] {self.name} deciding randomly since there are no previous states")
            chosen_action = Action(randint(0, len(Action) - 1))
        else:
            print(f"- previous state: {self.__previous_state.name}, previous_action: {self.__previous_action.name}")
            reward = self.__calculate_reward(curr_state)
            observed_value = self.__calculate_q_observed(reward, curr_state)
            temp_diff = self.__calculate_temporal_difference(observed_value)
            expected_q_value = self.__q_table[self.__previous_state.value, self.__previous_action.value]
            self.__update_rule(temp_diff)
            new_q_value = self.__q_table[self.__previous_state.value, self.__previous_action.value]
            print(
                f"- reward: {reward}, expected_q_value: {expected_q_value}, observed_q_value: {observed_value}, temporal difference: {temp_diff}")
            print(f"- updated q value [{self.__previous_state.name}, {self.__previous_action.name}]: {new_q_value}")

            if random() < self.__eps_curr:
                print(f"[ACTION] {self.name} deciding randomly...")
                chosen_action = Action(randint(0, len(Action) - 1))
            else:
                print(f"[ACTION] {self.name} deciding using q-table...")
                chosen_action = Action(self.__q_table[curr_state.value].argmax())

        self.__previous_state = curr_state
        self.__previous_action = chosen_action
        self.__eps_curr = max(self.__eps_min, self.__eps_curr * self.__eps_decay)

        base_action_params = self.__base_actions_map[chosen_action]
        act_dir = ActionDirection(chosen_action.value)
        print(f"[ACTION] {self.name} decided base action: {chosen_action.name} ({act_dir})")

        transform_action_params = self.__transform_base_action(base_action_params, restrictions, chosen_action)
        return chosen_action.name, act_dir, transform_action_params

    def load(self, model_path: Path) -> None:
        print(f"LOADING ({model_path})...", end="")
        agent = {}
        with open(model_path, "r", encoding="utf-8") as f:
            agent = json.load(f)

        self.__gamma_factor = agent["gamma"]
        self.__learning_rate = agent["learning_rate"]
        self.__eps_start = agent["epsilon"]["start"]
        self.__eps_curr = agent["epsilon"]["current"]
        self.__eps_decay = agent["epsilon"]["decay"]
        self.__eps_min = agent["epsilon"]["min"]
        self.__q_table = np.array(agent["q-table"])

        print("OK")

    def save(self, path: Path, file_name: str) -> Path:
        path.mkdir(parents=True, exist_ok=True)

        full_path = Path(f"{path}/{file_name}.{self.extension}")
        print(f"SAVING ({full_path})...", end="")

        agent = {
            "gamma": self.__gamma_factor,
            "learning_rate": self.__learning_rate,
            "epsilon": {
                "start": self.__eps_start,
                "current": self.__eps_curr,
                "decay": self.__eps_decay,
                "min": self.__eps_min,
            },
            "q-table": self.__q_table.tolist()
        }
        with open(full_path, "w", encoding="utf-8") as f:
            json.dump(agent, f, ensure_ascii=False, indent=4, sort_keys=True, separators=(",", ":"))
        print("OK")

        return full_path

    def print_model(self) -> None:
        print(f"-- {self.name} --")
        print(f"γ (gamma factor): {self.__gamma_factor}, α (learning rate): {self.__learning_rate}")
        print(
            f"ε (epsilon): {self.__eps_curr}/{self.__eps_start} (current/start), {self.__eps_decay} (decay), {self.__eps_min} (min)")
        print(
            f"state scores: {self.__min_score} (min, rounded to 4 decimals), {self.__max_score} (max, rounded to 4 decimals), intervals (min, max):")
        n_scores = len(self.__score_intervals)
        for key, val in self.__score_intervals.items():
            print(f"- {key}: ({val[0]}, {val[1]})")

        print(f"Q-Table:")
        self.print_q_table()

    def print_q_table(self) -> None:
        n_rows, n_cols = self.__q_table.shape

        print(f"{'':<15}|", end="")
        for action in range(n_rows):
            print(f" {Action(action).name:<15} |", end="")
        print("")

        for row in range(n_rows):
            print(f"{State(row).name:<15}|", end="")
            for col in range(n_cols):
                print(f" {self.__q_table[row][col]:<15} |", end="")
            print("")

    def print_scores(self, x: DataFrame) -> None:
        pass

    def __calculate_score(self, components: dict[str, str | int | float]) -> float:
        components_weighted = {key: (val * self.__score_weights[key]) if val is not None else None for key, val in
                               components.items()}

        score = components_weighted["has_completed_encounter"] - components_weighted["encounter_total_time_seconds"] + \
                components_weighted["player_average_accuracy"] - components_weighted["remaining_enemies"] + \
                components_weighted["remaining_player_health"] - components_weighted["total_hits_taken"]

        if components_weighted["average_time_between_kills_seconds"] is None:
            score -= 1 * self.__score_weights["average_time_between_kills_seconds"]
        else:
            score -= components_weighted["average_time_between_kills_seconds"]

        return score

    def __calculate_state(self, score: float) -> State | None:
        for state, interval in self.__score_intervals.items():
            if interval[0] < score <= interval[1]:
                return state

        return None

    def __calculate_reward(self, new_state: State) -> float:
        if self.__previous_state is None:
            raise ValueError("Previous state is None")

        base_reward = self.__base_rewards[new_state]
        prev_distance_to_balanced = abs(self.__previous_state.value - State.BALANCED.value)
        curr_distance_to_balanced = abs(new_state.value - State.BALANCED.value)

        final_reward = base_reward + self.__directional_bonus_reward * (
                curr_distance_to_balanced < prev_distance_to_balanced)
        return final_reward

    # Bellman's Equation
    def __calculate_q_observed(self, reward: float, new_state: State) -> float:
        return reward + self.__gamma_factor * self.__q_table[new_state.value].max()

    def __calculate_temporal_difference(self, observed_value: float) -> float:
        return observed_value - self.__q_table[self.__previous_state.value, self.__previous_action.value]

    def __update_rule(self, temporal_difference: float) -> None:
        self.__q_table[
            self.__previous_state.value, self.__previous_action.value] += self.__learning_rate * temporal_difference

    # Worst case is when player dies with all enemies remaining, 0 kills and 0 accuracy
    def __calculate_min_score(self) -> None:
        self.__min_score = round(-sqrt(
            self.__score_weights["encounter_total_time_seconds"] ** 2 + self.__score_weights["remaining_enemies"] ** 2 +
            self.__score_weights["total_hits_taken"] ** 2), 4)

    # Best case when player completes with 0 average_time_between_kills_seconds, 0 encounter_total_time_seconds, 100 player_average_accuracy, 100 remaining_player_health, 0 total_hits_taken
    def __calculate_max_score(self) -> None:
        self.__max_score = round(self.__score_weights["has_completed_encounter"] + sqrt(
            self.__score_weights["player_average_accuracy"] ** 2 + self.__score_weights[
                "remaining_player_health"] ** 2), 4)

    def __calculate_score_intervals(self) -> None:
        if self.__min_score is None or self.__max_score is None:
            raise ValueError("Limits for score are not defined")
        if self.__min_score > self.__max_score:
            raise ValueError("Min score is higher than max score")

        num_intervals = len(State)
        interval_range = (self.__max_score - self.__min_score) / num_intervals

        current_max_interval = self.__min_score + interval_range
        self.__score_intervals[State(0)] = (self.__min_score, current_max_interval)

        for i in range(1, num_intervals - 1):
            prev_max_interval = current_max_interval
            current_max_interval += interval_range
            self.__score_intervals[State(i)] = (prev_max_interval, current_max_interval)

        self.__score_intervals[State(num_intervals - 1)] = (current_max_interval, self.__max_score)

    def __transform_base_action(self, base_action: npt.NDArray[np.float32],
                                restrictions: dict[str, ObservationRestriction], action: Action) -> npt.NDArray[
        np.float32]:
        full_action = {}

        turret_restriction = cast(TurretNPCRestriction, restrictions[Restriction.TURRET_NPC])
        mobile_restriction = cast(MobileNPCRestriction, restrictions[Restriction.MOBILE_NPC])

        turret_count_step = 0
        turret_hp_step = 0.0
        turret_hitbox_step = 0.0
        mobile_count_step = 0
        mobile_hp_step = 0.0
        mobile_hitbox_step = 0.0
        if action != Action.KEEP_SAME:
            turret_count_step = calculate_additive_offset(turret_restriction.min_count,
                                                          turret_restriction.max_count,
                                                          turret_restriction.default_count,
                                                          turret_restriction.previous_count,
                                                          base_action[0],
                                                          0)
            turret_hp_step = calculate_additive_offset(turret_restriction.min_hp,
                                                       turret_restriction.max_hp,
                                                       turret_restriction.default_hp,
                                                       turret_restriction.previous_hp,
                                                       base_action[1],
                                                       1)
            turret_hitbox_step = calculate_additive_offset(turret_restriction.min_hitbox,
                                                           turret_restriction.max_hitbox,
                                                           turret_restriction.default_hitbox,
                                                           turret_restriction.previous_hitbox,
                                                           base_action[2], 2)

            mobile_count_step = calculate_additive_offset(mobile_restriction.min_count,
                                                          mobile_restriction.max_count,
                                                          mobile_restriction.default_count,
                                                          mobile_restriction.previous_count,
                                                          base_action[0],
                                                          0)

            mobile_hp_step = calculate_additive_offset(mobile_restriction.min_hp,
                                                       mobile_restriction.max_hp,
                                                       mobile_restriction.default_hp,
                                                       mobile_restriction.previous_hp,
                                                       base_action[1],
                                                       1)

            mobile_hitbox_step = calculate_additive_offset(mobile_restriction.min_hitbox,
                                                           mobile_restriction.max_hitbox,
                                                           mobile_restriction.default_hitbox,
                                                           mobile_restriction.previous_hitbox,
                                                           base_action[2],
                                                           2)

        steps = {
            "turret_count_#_step": turret_count_step,
            "turret_hp_#_step": turret_hp_step,
            "turret_hitbox_#_step": turret_hitbox_step,
            "mobile_count_#_step": mobile_count_step,
            "mobile_hp_#_step": mobile_hp_step,
            "mobile_hitbox_#_step": mobile_hitbox_step}

        for key, val in steps.items():
            is_float = isinstance(val, float)
            empty_val = 0.0 if is_float else 0
            if val > 0:
                full_action[key.replace("#", "add")] = val
                full_action[key.replace("#", "subtract")] = empty_val
            else:
                full_action[key.replace("#", "add")] = empty_val
                full_action[key.replace("#", "subtract")] = abs(val)

        print(f"[ACTION] Transformed based action: {str(full_action)}")
        return np.array([val for val in full_action.values()], dtype=np.float32)
