from pandas import DataFrame

from exceptions import AggregationAttributeComputeError
from shared import Event
from .aggregation_attribute import AggregationAttribute


class RemainingEnemiesAttribute(AggregationAttribute):

    def __init__(self) -> None:
        self.reset()

    @property
    def name(self) -> str:
        return "remaining_enemies"

    @property
    def value(self) -> str | int | float | None:
        return self.__value

    def reset(self) -> None:
        self.__value = None

    def process(self, events: dict[Event, DataFrame]) -> None:
        try:
            if len(events[Event.ENCOUNTER_ENDED]) < 1:
                return

            self.__value = events[Event.ENCOUNTER_ENDED].iloc[0]["remaining_enemies"]
        except Exception:
            raise AggregationAttributeComputeError(self.name)

    def finalize(self) -> None:
        pass
