from pydantic import BaseModel


class SetupSessionRequest(BaseModel):
    session_id: str
