from abc import abstractmethod, ABC
from pathlib import Path

import numpy as np
import numpy.typing as npt


class DDAModel(ABC):
    _output_path: Path
    _output_full_path: Path

    @property
    @abstractmethod
    def name(self) -> str:
        pass

    @property
    @abstractmethod
    def extension(self) -> str:
        pass

    def __init__(self, output_path: str) -> None:
        self._output_path = Path(output_path)

    @abstractmethod
    def act(self, observation: npt.NDArray[object]) -> npt.NDArray[np.float32]:
        pass

    @abstractmethod
    def load(self, model_path: str) -> None:
        pass

    @abstractmethod
    def save(self) -> None:
        pass

    @abstractmethod
    def print_model(self) -> None:
        pass
