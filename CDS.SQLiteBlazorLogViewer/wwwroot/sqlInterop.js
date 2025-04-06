let SQL;

window.loadSqlJsDb = async function (fileBytes) {
    if (!SQL) {
        SQL = await initSqlJs({
            locateFile: filename => `sqljs/${filename}`
        });
    }

    const uInt8Array = new Uint8Array(fileBytes);
    const db = new SQL.Database(uInt8Array);

    const result = db.exec("SELECT * FROM LogEntry ORDER BY DbId DESC LIMIT 2500");    

    if (result.length === 0) return [];

    const columns = result[0].columns;
    const values = result[0].values;

    return values.map(row =>
    {
        const obj = {};
        row.forEach((val, i) => obj[columns[i]] = val);
        return obj;
    });
};
