from fastapi import APIRouter

from dependencies import session_dependency
from requests import SetupSessionRequest

router = APIRouter()


@router.post("/session")
def setup_session(request: SetupSessionRequest, session_service: session_dependency):
    session_service.configure(request.session_id)
    return {
        "status": "success"
    }
