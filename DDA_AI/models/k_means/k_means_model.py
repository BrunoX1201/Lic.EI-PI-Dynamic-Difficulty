import pickle
from pathlib import Path
from typing import cast

import numpy as np
from numpy import typing as npt
from pandas import DataFrame, Series
from sklearn.cluster import KMeans
from sklearn.metrics import silhouette_score, davies_bouldin_score, calinski_harabasz_score

from shared import ObservationRestriction, Restriction, TurretNPCRestriction, MobileNPCRestriction
from utils import sort_clusters, calculate_additive_offset
from .player_experience import PlayerExperience
from ..fittable_model import FittableModel


class KMeansModel(FittableModel):
    __algorithm: KMeans

    __k: int
    __clusters_to_player_experience_map: dict[int, PlayerExperience]
    __encounter_classification: PlayerExperience | None

    # Multipliers:
    # - enemy_count: Higher values spawn more enemies (within each enemy type's restrictions).
    # - enemy_health: Higher values increase all enemies' health (within each enemy type's restrictions).
    # - enemy_hitbox: Higher values increase all enemies' hitbox size (within each enemy type's restrictions).
    __player_experience_to_base_actions_map: dict[PlayerExperience, npt.NDArray[np.float32]] = {
        PlayerExperience.BEGINNER: np.array([0.7, 0.7, 1.20], dtype=np.float32),
        PlayerExperience.INTERMEDIATE: np.array([0.9, 0.9, 1.15], dtype=np.float32),
        PlayerExperience.MEDIUM: np.array([1, 1, 1], dtype=np.float32),
        PlayerExperience.ADVANCED: np.array([1.1, 1.2, 0.90], dtype=np.float32),
        PlayerExperience.EXPERT: np.array([1.3, 1.4, 0.85], dtype=np.float32),
    }

    @property
    def name(self) -> str:
        return "k-means"

    @property
    def extension(self) -> str:
        return "pkl"

    def __init__(self) -> None:
        self.__k = 5
        self.__algorithm = KMeans(n_clusters=self.__k)
        self.__clusters_to_player_experience_map = {}
        self.__encounter_classification = None

    def fit(self, X: DataFrame) -> None:
        self.__algorithm.fit(X=X)
        self.__map_clusters_to_player_experience()

    def fit_with_labels(self, X: DataFrame, Y: Series) -> None:
        pass

    def act(self, observation: dict[str, str | int | float | dict[str, ObservationRestriction]]) -> tuple[
        str, npt.NDArray[
            np.float32]]:
        cp_observation = observation.copy()
        restrictions = cp_observation.pop("restrictions", None)
        if restrictions is None:
            raise KeyError(f"{self.name}: observation is incomplete")

        x = DataFrame([list(cp_observation.values())], columns=self.__algorithm.feature_names_in_)
        classification = self.__algorithm.predict(x)[0]
        self.__encounter_classification = self.__clusters_to_player_experience_map[classification]
        print(
            f"[ACTION] {self.name} classified {str(observation)} as {self.__encounter_classification.name} ({classification})")

        base_action = self.__player_experience_to_base_actions_map[self.__encounter_classification]
        print(f"[ACTION] {self.name} decided base action: {np.array2string(base_action)}")

        full_action = self.__transform_base_action(base_action, restrictions)
        return self.__encounter_classification.name, full_action

    def load(self, model_path: Path) -> None:
        print(f"LOADING ({model_path})...", end="")
        with open(model_path, "rb") as f:
            self.__algorithm = pickle.load(f)
        print("OK")

        self.__post_load_setup()

    def save(self, path: Path, file_name: str) -> Path:
        path.mkdir(parents=True, exist_ok=True)

        full_path = Path(f"{path}/{file_name}.{self.extension}")
        print(f"SAVING ({full_path})...", end="")
        with open(full_path, "wb") as f:
            pickle.dump(self.__algorithm, f, protocol=5)
        print("OK")

        return full_path

    def print_model(self) -> None:
        print(f"--- {self.name} ---")
        print(f"Total Iterations Ran: {self.__algorithm.n_iter_}")
        print(f"Clusters ({self.__k})")
        n_rows, n_cols = self.__algorithm.cluster_centers_.shape
        feature_names = self.__algorithm.get_feature_names_out()

        print(f"{'':<10}", end="")
        for i in range(n_cols):
            print(f" | {self.__algorithm.feature_names_in_[i]:<40} ", end="")
        print("")

        for i in range(n_rows):
            print(f"{feature_names[i]:<10}", end="")
            for j in range(n_cols):
                print(f" | {self.__algorithm.cluster_centers_[i, j]:<40}", end="")
            print("")

        print("Cluster Ranks")
        for i, (cluster, experience) in enumerate(self.__clusters_to_player_experience_map.items()):
            print(f"{i + 1}º - {cluster} ({experience.name})")

    def print_scores(self, x) -> None:
        silhouette = silhouette_score(x, self.__algorithm.labels_)
        db_index = davies_bouldin_score(x, self.__algorithm.labels_)
        ch_index = calinski_harabasz_score(x, self.__algorithm.labels_)

        print("--- Scores ---")
        print(f"Silhouette Score (closer to 1 is better): {silhouette:.2f}")
        print(f"Davies-Bouldin Index (lower score is better): {db_index:.2f}")
        print(f"Calinski-Harabasz Index (higher score is better): {ch_index:.2f}")

    def __post_load_setup(self) -> None:
        self.__k = self.__algorithm.n_clusters
        self.__map_clusters_to_player_experience()
        self.__encounter_classification = None

    def __map_clusters_to_player_experience(self) -> None:
        sorted_clusters = sort_clusters(self.__algorithm.cluster_centers_,
                                        [1.0, 0.5, 0.6, 0.3, 0.8, 0.9, 0.4],
                                        ["min", "min", "max", "max", "min", "max", "min"])

        for i, cluster in enumerate(sorted_clusters):
            self.__clusters_to_player_experience_map[cluster] = PlayerExperience((len(PlayerExperience) - 1) - i)

    def __transform_base_action(self, base_action: npt.NDArray[np.float32],
                                restrictions: dict[str, ObservationRestriction]) -> npt.NDArray[np.float32]:
        full_action = {}

        turret_restriction = cast(TurretNPCRestriction, restrictions[Restriction.TURRET_NPC])
        mobile_restriction = cast(MobileNPCRestriction, restrictions[Restriction.MOBILE_NPC])

        turret_count_step = 0
        turret_hp_step = 0.0
        turret_hitbox_step = 0.0
        mobile_count_step = 0
        mobile_hp_step = 0.0
        mobile_hitbox_step = 0.0
        if self.__encounter_classification != PlayerExperience.MEDIUM:
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
