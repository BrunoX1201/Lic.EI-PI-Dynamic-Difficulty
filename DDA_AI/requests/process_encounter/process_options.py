from pydantic import BaseModel


class ProcessOptions(BaseModel):
    rollback_on_success: bool = False
