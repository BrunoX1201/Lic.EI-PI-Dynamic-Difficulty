from abc import ABC, abstractmethod

from pandas import DataFrame

from shared import Event


class AggregationAttribute(ABC):
    @property
    @abstractmethod
    def name(self) -> str:
        pass

    @property
    @abstractmethod
    def value(self) -> str | int | float | None:
        pass

    @abstractmethod
    def reset(self) -> None:
        pass

    @abstractmethod
    def process(self, events: dict[Event, DataFrame]) -> None:
        pass

    @abstractmethod
    def finalize(self) -> None:
        pass

    def output(self) -> tuple[str, str | int | float | None]:
        return self.name, self.value
