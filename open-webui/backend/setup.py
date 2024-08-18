from cx_Freeze import setup, Executable
setup(
    name = "openwebui",
    version = "0.3.5",
    description = "openwebui",
    executables = [Executable("main.py")]
)