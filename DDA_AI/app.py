from fastapi import FastAPI, Depends

from dependencies import verify_session
from routers import session, encounter, dda_pipeline

app = FastAPI(title="Unity DDA Data Pipeline API", dependencies=[Depends(verify_session)])
app.include_router(session.router)
app.include_router(encounter.router)
app.include_router(dda_pipeline.router)

# Para correr o servidor diretamente pelo Python
if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="127.0.0.1", port=8000)
