from fastapi import APIRouter, HTTPException

from dependencies import dda_pipeline_dependency
from requests import UpdateDDAPipelineRequest

router = APIRouter()


@router.patch("/dda_pipeline")
def update_dda_pipeline(request: UpdateDDAPipelineRequest, dda_service: dda_pipeline_dependency):
    try:
        dda_service.decision_maker.change_model(request.selected_model)
        return {
            "status": "success",
        }

    except Exception as e:
        msg = str(e)
        print(f"[ERROR] {msg}")
        raise HTTPException(status_code=400, detail=msg)


@router.get("/dda_pipeline/models")
def get_dda_pipeline_models(dda_service: dda_pipeline_dependency):
    return {
        "available_models": dda_service.decision_maker.available_models,
        "selected_model": dda_service.decision_maker.selected_model.name,
    }
