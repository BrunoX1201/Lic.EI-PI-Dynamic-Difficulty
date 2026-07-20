from abc import abstractmethod

from pandas import DataFrame, Series

from .dda_model import DDAModel


class FittableModel(DDAModel):
    @abstractmethod
    def fit(self, X: DataFrame) -> None:
        pass

    @abstractmethod
    def fit_with_labels(self, X: DataFrame, Y: Series) -> None:
        pass
