from pathlib import Path

from pandas import DataFrame

from models.dda_model import DDAModel
from shared import ObservationRestriction


class DecisionStep:
    __available_models: dict[str, DDAModel]
    __selected_model: DDAModel
    __output: list[tuple[str, str | int | float]]

    @property
    def available_models(self) -> list[str]:
        return list(self.__available_models.keys())

    @property
    def selected_model(self) -> DDAModel:
        return self.__selected_model

    def __init__(self, available_models: list[DDAModel], selected_model: DDAModel) -> None:
        self.__available_models = {}
        for model in available_models:
            self.__available_models[model.name] = model

        self.__selected_model = selected_model

    def decide(self, observation: dict[str, str | int | float | ObservationRestriction]) -> list[
        tuple[str, str | int | float]]:
        self.__output = []
        action_name, output = self.__selected_model.act(observation)

        self.__output = [(key, val) for key, val in
                         zip(["turret_count_add_step", "turret_count_subtract_step", "turret_hp_add_step",
                              "turret_hp_subtract_step", "turret_hitbox_add_step", "turret_hitbox_subtract_step",
                              "mobile_count_add_step", "mobile_count_subtract_step", "mobile_hp_add_step",
                              "mobile_hp_subtract_step", "mobile_hitbox_add_step", "mobile_hitbox_subtract_step", ],
                             output)]
        self.__output.insert(0, ("action_name", action_name))
        
        return self.__output

    def save_output(self, path: Path, file_name: str, custom_columns: dict[str, str | int | float] = {}) -> Path:
        output_values = {}
        for key, value in self.__output:
            output_values[key] = value

        all_values_to_save = dict(custom_columns)
        all_values_to_save.update(output_values)
        df = DataFrame([all_values_to_save])

        path.mkdir(parents=True, exist_ok=True)
        full_path = Path(f"{path}/{file_name}.csv")

        output_file_exists = full_path.exists()
        df.to_csv(full_path, mode="a", header=not output_file_exists, index=False)
        return full_path

    def change_model(self, model: str) -> None:
        target = self.__available_models.get(model)
        if target is None:
            raise ValueError(f"Unknown model: {model}")

        self.__selected_model = target
