from pandas import DataFrame

from exceptions import AggregationAttributeComputeError
from shared import Event
from .aggregation_attribute import AggregationAttribute


class HasCompletedEncounterAttribute(AggregationAttribute):

    def __init__(self):
        self.reset()

    @property
    def name(self) -> str:
        return "has_completed_encounter"

    @property
    def value(self) -> str | int | float | None:
        return self.__value

    def reset(self) -> None:
        self.__value = None

    def process(self, events: dict[Event, DataFrame]) -> None:
        try:
            if len(events[Event.ENCOUNTER_ENDED]) < 1:
                return

            end_reason = events[Event.ENCOUNTER_ENDED].iloc[0]["end_reason"]
            self.__value = end_reason.upper() == "COMPLETED"
        except Exception:
            raise AggregationAttributeComputeError(self.name)

    def finalize(self) -> None:
        pass
