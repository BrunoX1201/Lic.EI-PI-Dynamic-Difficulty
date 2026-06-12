from dateutil.parser import parse
from pandas import DataFrame

from exceptions import AggregationAttributeComputeError
from shared import Event
from .aggregation_attribute import AggregationAttribute


class AverageTimeBetweenKillsAttribute(AggregationAttribute):

    def __init__(self):
        self.reset()

    @property
    def name(self) -> str:
        return "average_time_between_kills_seconds"

    @property
    def value(self) -> str | int | float | None:
        return self.__output

    def reset(self) -> None:
        self.__output = None

        self.__encounter_start_time = None
        self.__has_gotten_encounter_info = False

        self.__num_rows = 1  # The encounter start row
        self.__total_time_between_kills = 0
        self.__previous_kill_timestamp = None

        self.__has_processed_first_kill = False

    def process(self, events: dict[Event, DataFrame]) -> None:
        try:
            if not self.__has_gotten_encounter_info:
                self.__encounter_start_time = parse(events[Event.ENCOUNTER_STARTED].iloc[0]["timestamp"])

                self.__has_gotten_encounter_info = True

            num_rows = len(events[Event.TARGET_KILLED])
            if num_rows < 1:
                return
            self.__num_rows += num_rows

            start_index = 0
            if not self.__has_processed_first_kill:
                first_kill_time = parse(events[Event.TARGET_KILLED].iloc[0]["timestamp"])
                self.__total_time_between_kills += (first_kill_time - self.__encounter_start_time).total_seconds()
                self.__previous_kill_timestamp = first_kill_time
                start_index = 1

                self.__has_processed_first_kill = True

            kill_timestamps = events[Event.TARGET_KILLED]["timestamp"]
            for i in range(start_index, num_rows):
                current_kill_time = parse(kill_timestamps.iloc[i])
                self.__total_time_between_kills += (current_kill_time - self.__previous_kill_timestamp).total_seconds()
                self.__previous_kill_timestamp = current_kill_time

        except Exception:
            raise AggregationAttributeComputeError(self.name)

    def finalize(self) -> None:
        try:
            # Output is only different from None if at least one kill was made
            if self.__num_rows > 1:
                self.__output = self.__total_time_between_kills / (self.__num_rows - 1)
        except Exception:
            raise AggregationAttributeComputeError(self.name)
