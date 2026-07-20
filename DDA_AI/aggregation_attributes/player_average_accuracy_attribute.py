from pandas import DataFrame

from exceptions import AggregationAttributeComputeError
from shared import Event
from .aggregation_attribute import AggregationAttribute


class PlayerAverageAccuracyAttribute(AggregationAttribute):
    
    def __init__(self) -> None:
        self.reset()

    @property
    def name(self) -> str:
        return "player_average_accuracy"

    @property
    def value(self) -> str | int | float | None:
        return self.__output

    def reset(self) -> None:
        self.__output = None
        self.__num_rows = 0
        self.__sum_values = 0

    def process(self, events: dict[Event, DataFrame]) -> None:
        try:
            num_rows = len(events[Event.PLAYER_ATTACKED])
            if num_rows < 1:
                return

            self.__num_rows += num_rows
            self.__sum_values += events[Event.PLAYER_ATTACKED]["accuracy"].sum()
        except Exception:
            raise AggregationAttributeComputeError(self.name)

    def finalize(self) -> None:
        try:
            # Output is only different from None if at least one attack was made
            if self.__num_rows > 0:
                self.__output = self.__sum_values / self.__num_rows
        except Exception:
            raise AggregationAttributeComputeError(self.name)
