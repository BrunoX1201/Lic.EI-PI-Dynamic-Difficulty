class Session:
    __id: str | None
    __is_configured: bool

    def __init__(self) -> None:
        self.__id = None
        self.__is_configured = False

    @property
    def id(self) -> str:
        return self.__id

    @property
    def is_configured(self) -> bool:
        return self.__is_configured

    def configure(self, session_id: str) -> None:
        self.__id = session_id
        self.__is_configured = True
