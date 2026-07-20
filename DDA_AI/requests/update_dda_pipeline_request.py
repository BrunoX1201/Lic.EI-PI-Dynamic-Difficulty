from pydantic import BaseModel


class UpdateDDAPipelineRequest(BaseModel):
    selected_model: str
