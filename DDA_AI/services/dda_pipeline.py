from steps import DataPreparationStep


class DDAPipeline:
    __data_preparation: DataPreparationStep

    @property
    def data_preparation(self) -> DataPreparationStep:
        return self.__data_preparation
    
    def __init__(self, data_preparation: DataPreparationStep) -> None:
        self.__data_preparation = data_preparation
