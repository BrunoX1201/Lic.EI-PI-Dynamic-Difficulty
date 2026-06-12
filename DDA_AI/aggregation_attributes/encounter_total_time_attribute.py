from dateutil.parser import parse
from pandas import DataFrame

from exceptions import AggregationAttributeComputeError
from shared import Event
from .aggregation_attribute import AggregationAttribute


class EncounterTotalTimeAttribute(AggregationAttribute):

    def __init__(self):
        self.reset()

    @property
    def name(self) -> str:
        return "encounter_total_time_seconds"

    @property
    def value(self) -> str | int | float | None:
        return self.__value

    def reset(self) -> None:
        self.__value = None

    def process(self, events: dict[Event, DataFrame]) -> None:
        try:
            if len(events[Event.ENCOUNTER_STARTED]) < 1 or len(events[Event.ENCOUNTER_ENDED]) < 1:
                return

            start = parse(events[Event.ENCOUNTER_STARTED].iloc[0]["timestamp"])
            end = parse(events[Event.ENCOUNTER_ENDED].iloc[0]["timestamp"])
            total_time = (end - start)

            self.__value = total_time.total_seconds()
        except Exception:
            raise AggregationAttributeComputeError(self.name)

    def finalize(self) -> None:
        pass
