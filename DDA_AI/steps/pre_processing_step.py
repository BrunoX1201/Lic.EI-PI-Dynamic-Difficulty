import os
from datetime import datetime

from pandas import DataFrame, read_csv, Series
from pandas.errors import EmptyDataError

from aggregation_attributes import AggregationAttribute
from exceptions import EncounterStartLimitNotFound, EncounterEndLimitNotFound
from shared import Event
from utils import event_attributes_map


class PreProcessingStep:
    __event_base_path: str
    __event_files: dict[Event, str] = {
        Event.ENCOUNTER_ENDED: "encounter_ended_event.csv",
        Event.ENCOUNTER_STARTED: "encounter_started_event.csv",
        Event.ENCOUNTER_ZONE_ENTERED: "encounter_zone_entered_event.csv",
        Event.ENCOUNTER_ZONE_LEFT: "encounter_zone_left_event.csv",
        Event.GAME_TIME_PASSED: "game_time_passed_event.csv",
        Event.ITEM_PICKED_UP: "item_picked_up_event.csv",
        Event.NEW_LOCATION_DISCOVERED: "new_location_discovered_event.csv",
        Event.PLAYER_ATTACKED: "player_attacked_event.csv",
        Event.PLAYER_DIED: "player_died_event.csv",
        Event.PLAYER_TOOK_DAMAGE: "player_took_damage_event.csv",
        Event.PLAYER_WENT_AIRBORNE: "player_went_airborne_event.csv",
        Event.TARGET_KILLED: "target_killed_event.csv",
    }
    __event_full_paths: dict[Event, str]
    __loaded_events: dict[Event, DataFrame]
    __events_to_load: list[Event]
    __event_num_rows_read: dict[Event, dict[str, int]]

    # Internal Metrics
    __total_lines_read: int
    __total_valid_lines_read: int

    __encounter_start: DataFrame
    __encounter_end: DataFrame

    __batch_size: int
    __aggregation_attributes: list[AggregationAttribute]

    __output: list[tuple[str, str | int | float | None]]
    __output_path: str
    __output_file: str
    __output_full_path: str

    def __init__(
            self,
            event_base_path: str,
            output_path: str,
            output_file: str,
            aggregation_attributes: list[AggregationAttribute],
            batch_size: int = 10,
    ) -> None:
        self.__event_base_path = event_base_path
        self.__event_full_paths = {}
        for eventKey, eventFile in self.__event_files.items():
            self.__event_full_paths[eventKey] = self.__event_base_path + eventFile

        self.__output_path = output_path
        self.__output_file = output_file
        self.__output_full_path = os.path.join(output_path, output_file + ".csv")
        self.__batch_size = batch_size
        self.__aggregation_attributes = aggregation_attributes

        self.__event_num_rows_read = {}
        self.__total_lines_read = 0
        self.__total_valid_lines_read = 0
        for key in self.__event_files.keys():
            self.__event_num_rows_read[key] = {
                "total": 0,
                "total_valid": 0,
                "call_total": 0,
                "call_total_valid": 0
            }

        self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["totalInvalidLines"] = 0
        self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["totalInvalidLines"] = 0

    def transform(self, encounter_id: str) -> list[tuple[str, str | int | float | None]]:
        self.__reset()

        startTotalLinesStartLimit = self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["total"]
        startTotalLinesEndLimit = self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["total"]
        try:
            self.__encounter_start = self.__find_encounter_start(encounter_id)
            self.__encounter_end = self.__find_encounter_end(
                encounter_id, self.__encounter_start.iloc[0]["timestamp"]
            )
        except Exception as e:
            print("\r\nSomething went wrong.")
            print(e)

            print("-- Rollbacking Limits Batcher ---")
            print("Previous SkipRows")
            print(f"- Start Limit: {self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["total"]}")
            print(f"- End Limit: {self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["total"]}")
            numInvalidLinesStartLimit = self.__event_num_rows_read[Event.ENCOUNTER_STARTED][
                                            "total"] - startTotalLinesStartLimit
            numInvalidLinesEndLimit = self.__event_num_rows_read[Event.ENCOUNTER_ENDED][
                                          "total"] - startTotalLinesEndLimit

            self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["totalInvalidLines"] += numInvalidLinesStartLimit
            self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["totalInvalidLines"] += numInvalidLinesEndLimit

            print("Current SkipRows")
            print(
                f"- Start Limit: {self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["total"] - self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["totalInvalidLines"]}")
            print(
                f"- End Limit: {self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["total"] - self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["totalInvalidLines"]}")

            return []

        print(f"\r\n\r\n--- Start ---\r\n{self.__encounter_start.to_string()}")
        print(f"\r\n--- End ---\r\n{self.__encounter_end.to_string()}")

        print("\r\n--- Pre Iteration ---")
        self.__loaded_events[Event.ENCOUNTER_STARTED] = self.__encounter_start
        self.__loaded_events[Event.ENCOUNTER_ENDED] = self.__encounter_end
        print(f"loading {Event.ENCOUNTER_STARTED}...", end="")
        print("COMPLETED")
        print(f"loading {Event.ENCOUNTER_ENDED}...", end="")
        print("COMPLETED")

        iteration = 1
        start_limit = self.__encounter_start.iloc[0]["timestamp"]
        end_limit = self.__encounter_end.iloc[0]["timestamp"]

        try:
            while len(self.__events_to_load) != 0:
                print(
                    f"\r\n--- Iteration {iteration} ({len(self.__event_files) - len(self.__events_to_load)} / {len(self.__event_files)}) ---")
                events_completed_from_empty_data, events_completed_from_limit_reached = self.__load_events(start_limit,
                                                                                                           end_limit)

                for evt in events_completed_from_empty_data:
                    self.__loaded_events[evt] = self.__make_empty_event_df(evt)
                    self.__events_to_load.remove(evt)

                self.__compute_attributes()

                for evt in events_completed_from_limit_reached:
                    self.__loaded_events[evt] = self.__make_empty_event_df(evt)
                    self.__events_to_load.remove(evt)

                iteration += 1

            for attr in self.__aggregation_attributes:
                attr.finalize()
                self.__output.append(attr.output())

            self.__print_statistics()

        except Exception as e:
            print("\r\nSomething went wrong!")
            print(e)

        return self.__output

    def __reset(self) -> None:
        self.__output = []
        self.__loaded_events = {}
        self.__events_to_load = []

        for stats in self.__event_num_rows_read.values():
            stats["call_total"] = 0
            stats["call_total_valid"] = 0

        for key in self.__event_files.keys():
            self.__events_to_load.append(key)

        # Removing start encounter and end encounter events since these are already loaded
        self.__events_to_load.remove(Event.ENCOUNTER_STARTED)
        self.__events_to_load.remove(Event.ENCOUNTER_ENDED)

        for attr in self.__aggregation_attributes:
            attr.reset()

    def __find_encounter_start(self, encounter_id: str) -> DataFrame:
        has_found = False
        target = None
        while not has_found:
            encounter_starts = read_csv(
                filepath_or_buffer=self.__event_full_paths[Event.ENCOUNTER_STARTED],
                header=None,
                names=event_attributes_map[Event.ENCOUNTER_STARTED].keys(),
                dtype=event_attributes_map[Event.ENCOUNTER_STARTED],
                nrows=self.__batch_size,
                skiprows=self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["total"] -
                         self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["totalInvalidLines"],
                parse_dates=["timestamp"],
                delimiter=";",
            )
            num_rows = len(encounter_starts)
            if num_rows < 1:
                raise EncounterStartLimitNotFound(f"Could not find start of encounter ({encounter_id})")

            encounters = encounter_starts.loc[encounter_starts["encounter_id"] == encounter_id]

            if encounters.size < 1:
                self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["total"] += num_rows
                self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["call_total"] += num_rows
                continue

            encounters.sort_values(by=["timestamp"], inplace=True, ascending=False)
            target = encounters.iloc[:1]
            has_found = True

            first_row_index = encounter_starts.index[0]
            target_row_index = target.index[0]
            true_num_rows = target_row_index - first_row_index + 1

            self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["total"] += true_num_rows
            self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["call_total"] += true_num_rows
            self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["total_valid"] += 1
            self.__event_num_rows_read[Event.ENCOUNTER_STARTED]["call_total_valid"] += 1
            self.__total_lines_read += true_num_rows
            self.__total_valid_lines_read += 1

        return target

    def __find_encounter_end(self, encounter_id: str, start_timestamp: datetime) -> DataFrame:
        has_found = False
        target = None
        while not has_found:
            encounter_ends = read_csv(
                filepath_or_buffer=self.__event_full_paths[Event.ENCOUNTER_ENDED],
                header=None,
                names=event_attributes_map[Event.ENCOUNTER_ENDED].keys(),
                dtype=event_attributes_map[Event.ENCOUNTER_ENDED],
                nrows=self.__batch_size,
                skiprows=self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["total"] -
                         self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["totalInvalidLines"],
                parse_dates=["timestamp"],
                delimiter=";",
            )
            num_rows = len(encounter_ends)
            if num_rows < 1:
                raise EncounterEndLimitNotFound(f"Could not find start of encounter ({encounter_id})")

            encounters = encounter_ends[
                (encounter_ends["encounter_id"] == encounter_id)
                & (encounter_ends["timestamp"] >= start_timestamp)
                ]

            if encounters.size < 1:
                self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["total"] += num_rows
                self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["call_total"] += num_rows
                continue

            encounters.sort_values(by=["timestamp"], inplace=True)
            target = encounters.iloc[:1]
            has_found = True

            first_row_index = encounter_ends.index[0]
            target_row_index = target.index[0]
            true_num_rows = target_row_index - first_row_index + 1

            self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["total"] += true_num_rows
            self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["call_total"] += true_num_rows
            self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["total_valid"] += 1
            self.__event_num_rows_read[Event.ENCOUNTER_ENDED]["call_total_valid"] += 1
            self.__total_lines_read += true_num_rows
            self.__total_valid_lines_read += 1

        return target

    def __load_events(self, start_limit: datetime, end_limit: datetime) -> tuple[list[Event], list[Event]]:
        events_completed_from_empty_data = []
        events_completed_from_limit_reached = []
        for event in self.__events_to_load:
            print(f"loading {event}...", end="")

            try:
                loaded_events = read_csv(
                    filepath_or_buffer=self.__event_full_paths[event],
                    header=None,
                    names=event_attributes_map[event].keys(),
                    dtype=event_attributes_map[event],
                    delimiter=";",
                    nrows=self.__batch_size,
                    skiprows=self.__event_num_rows_read[event]["total"],
                    parse_dates=["timestamp"]
                )

                num_rows_read = len(loaded_events)
                if num_rows_read == 0:
                    raise EmptyDataError

                num_rows_after_end_limit = len(loaded_events[loaded_events["timestamp"] > end_limit])
                num_rows_read -= num_rows_after_end_limit

                self.__loaded_events[event] = loaded_events[
                    (loaded_events["timestamp"] >= start_limit) &
                    (loaded_events["timestamp"] <= end_limit)
                    ]
                num_valid_rows = len(self.__loaded_events[event])

                self.__event_num_rows_read[event]["total"] += num_rows_read
                self.__event_num_rows_read[event]["call_total"] += num_rows_read
                self.__total_lines_read += num_rows_read

                self.__event_num_rows_read[event]["total_valid"] += num_valid_rows
                self.__event_num_rows_read[event]["call_total_valid"] += num_valid_rows
                self.__total_valid_lines_read += num_valid_rows

                if num_rows_after_end_limit > 0:
                    print("COMPLETED (End limit reached)")
                    events_completed_from_limit_reached.append(event)
                else:
                    print(f"OK ({num_valid_rows}/{num_rows_read})")
            except EmptyDataError:
                print("COMPLETED (No more rows)")
                events_completed_from_empty_data.append(event)

        return events_completed_from_empty_data, events_completed_from_limit_reached

    def __compute_attributes(self) -> None:
        print("Current values:")
        for attribute in self.__aggregation_attributes:
            attribute.process(self.__loaded_events)
            print(f"{attribute.name}: {attribute.value}")

    def save_output(self) -> None:
        values = {"session_id": self.__encounter_start.iloc[0]["session_id"],
                  "encounter_id": self.__encounter_start.iloc[0]["encounter_id"],
                  "encounter_start_timestamp": self.__encounter_start.iloc[0]["timestamp"],
                  "encounter_end_timestamp": self.__encounter_end.iloc[0]["timestamp"]}

        for attr_key, attr_value in self.__output:
            values[attr_key] = attr_value

        data_frame = DataFrame([values])

        if not os.path.exists(self.__output_path):
            os.makedirs(self.__output_path)

        output_file_exists = os.path.exists(self.__output_full_path)
        data_frame.to_csv(self.__output_full_path, mode="a", header=not output_file_exists, index=False)

    def __print_statistics(self) -> None:
        print("\r\n--- Statistics ---")
        print(f"Total files searched: {len(self.__event_files)}")
        print(f"Total lines (valid / total): {self.__total_valid_lines_read} / {self.__total_lines_read} ")
        print(f"Total lines per event (valid / total):")
        print(f"{'':<35} {'Iteration':<15} {'Session':<15}")

        for key, stats in self.__event_num_rows_read.items():
            iteration = f"{stats["call_total_valid"]} / {stats['call_total']}"
            session = f"{stats["total_valid"]} / {stats["total"]}"
            print(f"{key:<35} {iteration:<15} {session:<15}")

        print("Aggregation Attributes:")
        for attr_name, attr_value in self.__output:
            print(f"{attr_name}: {attr_value}")

    @staticmethod
    def __make_empty_event_df(event: Event) -> DataFrame:
        columns = {}
        for column, column_type in event_attributes_map[event].items():
            columns[column] = Series(dtype=column_type)
        return DataFrame(data=columns)
