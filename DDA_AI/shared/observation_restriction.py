from abc import ABC, abstractmethod


class ObservationRestriction(ABC):

    @abstractmethod
    def __str__(self) -> str:
        pass

    def __repr__(self) -> str:
        return self.__str__()
