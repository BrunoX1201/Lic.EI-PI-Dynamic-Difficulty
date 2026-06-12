from pandas import DataFrame

from exceptions import AggregationAttributeComputeError
from shared import Event
from .aggregation_attribute import AggregationAttribute


class TotalHitsTakenAttribute(AggregationAttribute):

    def __init__(self):
        self.reset()

    @property
    def name(self) -> str:
        return "total_hits_taken"

    @property
    def value(self) -> str | int | float | None:
        return self.__value

    def reset(self) -> None:
        self.__value = None

    def process(self, events: dict[Event, DataFrame]) -> None:
        try:
            hits_taken = len(events[Event.PLAYER_TOOK_DAMAGE])
            if self.__value is None:
                self.__value = 0

            self.__value += hits_taken

        except Exception:
            raise AggregationAttributeComputeError(self.name)

    def finalize(self) -> None:
        pass
