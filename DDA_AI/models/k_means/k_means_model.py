import datetime
import math
import pickle
from pathlib import Path

import numpy as np
from numpy import typing as npt
from pandas import DataFrame, Series
from sklearn.cluster import KMeans

from models.fittable_model import FittableModel
from models.k_means.player_experience import PlayerExperience
from utils import sort_clusters


class KMeansModel(FittableModel):
    __algorithm: KMeans

    __k: int
    __clusters_to_player_experience_map: dict[int, PlayerExperience]

    # [total_enemies, enemy_health, enemy_hitbox]
    __player_experience_to_actions_map = {
        PlayerExperience.BEGINNER: np.array([0.5, 0.75, 1.2], dtype=np.float32),
        PlayerExperience.INTERMEDIATE: np.array([0.9, 1, 1], dtype=np.float32),
        PlayerExperience.MEDIUM: np.array([1.2, 1.4, 0.85], dtype=np.float32),
        PlayerExperience.ADVANCED: np.array([1.6, 1.8, 0.75], dtype=np.float32),
        PlayerExperience.EXPERT: np.array([2, 2, 0.65], dtype=np.float32),
    }

    @property
    def name(self) -> str:
        return "k-means"

    @property
    def extension(self) -> str:
        return "pkl"

    def __init__(self, model_path: str) -> None:
        super().__init__(model_path)
        self.__k = 5
        self.__algorithm = KMeans(n_clusters=self.__k)
        self.__clusters_to_player_experience_map = {}

    def fit(self, X: DataFrame) -> None:
        self.__algorithm.fit(X=X)
        self.__map_clusters_to_player_experience()

    def fit_with_labels(self, X: DataFrame, Y: Series) -> None:
        pass

    def act(self, observation: npt.NDArray[np.float64]) -> npt.NDArray[np.float32]:

        classification = self.__algorithm.predict([observation])[0]
        experience = self.__clusters_to_player_experience_map[classification]
        print(f"[ACTION] {self.name} classified {np.array2string(observation)} as {experience.name} ({classification})")

        action = self.__player_experience_to_actions_map[experience]
        print(f"[ACTION] {self.name} decided action: {np.array2string(action)}")
        return action

    def load(self, model_path: str) -> None:
        print(f"LOADING ({model_path})...", end="")
        with open(model_path, "rb") as f:
            self.__algorithm = pickle.load(f)
        print("OK")

        self.__post_load_setup()

    def save(self) -> str:
        self._output_path.mkdir(parents=True, exist_ok=True)

        utc_date = datetime.datetime.now(datetime.timezone.utc)
        timestamp = math.floor(utc_date.timestamp())
        self._output_full_path = Path(f"{self._output_path}/{self.name}_{timestamp}.{self.extension}")
        print(f"SAVING ({self._output_full_path})...", end="")
        with open(self._output_full_path, "wb") as f:
            pickle.dump(self.__algorithm, f, protocol=5)
        print("OK")

        return str(self._output_full_path)

    def print_model(self) -> None:
        print(f"--- {self.name} ---")
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

    def __post_load_setup(self) -> None:
        self.__k = self.__algorithm.n_clusters
        self.__map_clusters_to_player_experience()

    def __map_clusters_to_player_experience(self) -> None:
        sorted_clusters = sort_clusters(self.__algorithm.cluster_centers_,
                                        [0.5, 0.6, 1.0, 0.3, 0.8, 0.9, 0.4],
                                        ["min", "min", "max", "max", "min", "max", "min"])

        for i, cluster in enumerate(sorted_clusters):
            self.__clusters_to_player_experience_map[cluster] = PlayerExperience((len(PlayerExperience) - 1) - i)
