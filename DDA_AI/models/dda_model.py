from abc import abstractmethod, ABC
from pathlib import Path

import numpy as np
import numpy.typing as npt
from pandas import DataFrame

from shared import ObservationRestriction, ActionDirection


class DDAModel(ABC):
    @property
    @abstractmethod
    def name(self) -> str:
        pass

    @property
    @abstractmethod
    def extension(self) -> str:
        pass

    @abstractmethod
    def act(self, observation: dict[str, str | int | float | ObservationRestriction]) -> tuple[
        str, ActionDirection, npt.NDArray[np.float32]]:
        pass

    @abstractmethod
    def load(self, model_path: Path) -> None:
        pass

    @abstractmethod
    def save(self, path: Path, file_name: str) -> Path:
        pass

    @abstractmethod
    def print_model(self) -> None:
        pass

    @abstractmethod
    def print_scores(self, x: DataFrame) -> None:
        pass
