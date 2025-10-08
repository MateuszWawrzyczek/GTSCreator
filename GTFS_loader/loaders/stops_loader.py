import pandas as pd
import io
import csv
from db import get_connection

def load_stops(feed_id: str, file, progress_callback=None):
    conn = get_connection()
    cur = conn.cursor()

    # Wczytaj CSV z pliku GTFS
    df = pd.read_csv(file)
    df["feed_id"] = feed_id

    # Upewnij się, że kolumny są w odpowiedniej kolejności
    df = df[["feed_id", "stop_id", "stop_name", "stop_code", "stop_lon", "stop_lat"]]

    # Zamień tabulatory i znaki nowej linii w danych (bezpieczniejsze)
    df = df.replace({"\t": " ", "\n": " "}, regex=True)

    # Przygotuj bufor TSV (tab-separated values)
    buffer = io.StringIO()
    df.to_csv(
        buffer,
        index=False,
        header=False,
        sep="\t",                # TAB jako separator
        quoting=csv.QUOTE_NONE   # bez cudzysłowów
    )
    buffer.seek(0)

    # Usuń stare dane (dla bezpieczeństwa)
    cur.execute("DELETE FROM stops WHERE feed_id = %s;", (feed_id,))

    # Szybki COPY z tabulatorem
    cur.copy_from(
        buffer,
        "stops",
        sep="\t",
        columns=("feed_id", "stop_id", "stop_name", "stop_code", "stop_lon", "stop_lat")
    )

    conn.commit()
    cur.close()
    conn.close()

    if progress_callback:
        progress_callback(len(df))
