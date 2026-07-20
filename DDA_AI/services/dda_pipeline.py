from steps import DataPreparationStep, DecisionStep


class DDAPipeline:
    __data_preparation: DataPreparationStep
    __decision_maker: DecisionStep

    @property
    def data_preparation(self) -> DataPreparationStep:
        return self.__data_preparation

    @property
    def decision_maker(self) -> DecisionStep:
        return self.__decision_maker

    def __init__(self, data_preparation: DataPreparationStep, decision_maker: DecisionStep) -> None:
        self.__data_preparation = data_preparation
        self.__decision_maker = decision_maker
