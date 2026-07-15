from pydantic import BaseModel


class SetupSessionRequest(BaseModel):
    session_id: str
    telemetry_base_path: str | None = None
