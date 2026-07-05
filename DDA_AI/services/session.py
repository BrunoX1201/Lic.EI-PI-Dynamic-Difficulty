from pathlib import Path


class Session:
    __OUTPUT_BASE_PATH: Path = Path("./output/runtime")
    __output_path: Path

    __id: str | None
    __is_configured: bool

    def __init__(self) -> None:
        self.__id = None
        self.__is_configured = False
        self.__output_path = self.__OUTPUT_BASE_PATH

    @property
    def id(self) -> str:
        return self.__id

    @property
    def is_configured(self) -> bool:
        return self.__is_configured

    @property
    def output_path(self) -> Path:
        return self.__output_path

    def configure(self, session_id: str) -> None:
        self.__update_id(session_id)
        self.__is_configured = True

    def __update_id(self, session_id: str) -> None:
        self.__id = session_id
        self.__output_path = Path(f"{self.__OUTPUT_BASE_PATH}/session_{self.__id}")
