
import logging
import typing as _t


def get_console_logger(name: _t.Optional[str] = None, level: _t.Optional[int] = logging.INFO) -> logging.Logger:
    """Get simple console logger with custom name"""
    logger = logging.getLogger(name)
    handler = logging.StreamHandler()
    formatter = logging.Formatter("[%(asctime)s] [%(name)s] [%(levelname)s] - %(message)s")
    handler.setFormatter(formatter)
    logger.setLevel(level)
    logger.addHandler(handler)

    return logger
