from fastapi import FastAPI

from routers import session, encounter

app = FastAPI(title="Unity DDA Data Pipeline API")
app.include_router(session.router)
app.include_router(encounter.router)

# Para correr o servidor diretamente pelo Python
if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="127.0.0.1", port=8000)
