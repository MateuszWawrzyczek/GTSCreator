import pandas as pd
from db import get_connection

def load_agency(feed_id: str, filepath: str):
    conn = get_connection()
    cur = conn.cursor()

    df = pd.read_csv(filepath)

    for _, row in df.iterrows():
        cur.execute("""
            INSERT INTO agency (feed_id, agency_id, agency_name, agency_url, agency_timezone, agency_lang, agency_phone)
            VALUES (%s, %s, %s, %s, %s, %s, %s)
            ON CONFLICT (agency_id) DO NOTHING;
        """, (
            feed_id,
            row["agency_id"],
            row["agency_name"],
            row["agency_url"],
            row["agency_timezone"],
            row.get("agency_lang"),
            row.get("agency_phone")
        ))

    conn.commit()
    cur.close()
    conn.close()
