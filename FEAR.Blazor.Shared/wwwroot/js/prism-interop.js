function activatePrismHighlighting() {
    // Prism SQL language definition (kept as a global assignment for compatibility)
    Prism.languages.sparql = {
        'comment': {
            pattern: /(^|[^\\<])(?:\/\*[\s\S]*?\*\/|(?:--|\/\/|#)(?![^\n<>]*>).*?)/,
            lookbehind: true
        },
        'string': {
            pattern: /(^|[^@@\\])("|')(?:\\[\s\S]|(?!\2)[^\\])*\2/,
            greedy: true,
            lookbehind: true
        },
        'uri': {
            pattern: /(^|[^@@\\])(<[^>\n]*>)/,
            greedy: true,
            lookbehind: true
        },
        'suri': {
            pattern: /(^|[^@@\\])\b[a-zA-Z_][a-zA-Z0-9_.-]*:[a-zA-Z0-9_.-]+\b/,
            lookbehind: true
        },
        'variable': /\?[\w.$]+|\?(["'`])(?:\\[\s\S]|(?!\1)[^\\])+\1/,
        'function': /\b(?:AVG|COUNT|FIRST|FORMAT|LAST|LCASE|LEN|MAX|MID|MIN|MOD|NOW|ROUND|SUM|UCASE)(?=\s*\()/i,
        'keyword': /\b(?:PREFIX|CONSTRUCT|FILTER|EXISTS|ACTION|ADD|AFTER|ALGORITHM|ALL|ALTER|ANALYZE|ANY|APPLY|AS|ASC|AUTHORIZATION|AUTO_INCREMENT|BACKUP|BDB|BEGIN|BERKELEYDB|BIGINT|BINARY|BIT|BLOB|BOOL|BOOLEAN|BREAK|BROWSE|BTREE|BULK|BY|CALL|CASCADED?|CASE|CHAIN|CHAR(?:ACTER|SET)?|CHECK(?:POINT)?|CLOSE|CLUSTERED|COALESCE|COLLATE|COLUMNS?|COMMENT|COMMIT(?:TED)?|COMPUTE|CONNECT|CONSISTENT|CONSTRAINT|CONTAINS(?:TABLE)?|CONTINUE|CONVERT|CREATE|CROSS|CURRENT(?:_DATE|_TIME|_TIMESTAMP|_USER)?|CURSOR|CYCLE|DATA(?:BASES?)?|DATE(?:TIME)?|DAY|DBCC|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFINER|DELAYED|DELETE|DELIMITERS?|DENY|DESC|DESCRIBE|DETERMINISTIC|DISABLE|DISCARD|DISK|DISTINCT|DISTINCTROW|DISTRIBUTED|DO|DOUBLE|DROP|DUMMY|DUMP(?:FILE)?|DUPLICATE|ELSE(?:IF)?|ENABLE|ENCLOSED|END|ENGINE|ENUM|ERRLVL|ERRORS|ESCAPED?|EXCEPT|EXEC(?:UTE)?|EXISTS|EXIT|EXPLAIN|EXTENDED|FETCH|FIELDS|FILE|FILLFACTOR|FIRST|FIXED|FLOAT|FOLLOWING|FOR(?: EACH ROW)?|FORCE|FOREIGN|FREETEXT(?:TABLE)?|FROM|FULL|FUNCTION|GEOMETRY(?:COLLECTION)?|GLOBAL|GOTO|GRANT|GROUP|HANDLER|HASH|HAVING|HOLDLOCK|HOUR|IDENTITY(?:_INSERT|COL)?|IF|IGNORE|IMPORT|INDEX|INFILE|INNER|INNODB|INOUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|INVOKER|ISOLATION|ITERATE|JOIN|KEYS?|KILL|LANGUAGE|LAST|LEAVE|LEFT|LEVEL|LIMIT|LINENO|LINES|LINESTRING|LOAD|LOCAL|LOCK|LONG(?:BLOB|TEXT)|LOOP|MATCH(?:ED)?|MEDIUM(?:BLOB|INT|TEXT)|MERGE|MIDDLEINT|MINUTE|MODE|MODIFIES|MODIFY|MONTH|MULTI(?:LINESTRING|POINT|POLYGON)|NATIONAL|NATURAL|NCHAR|NEXT|NO|NONCLUSTERED|NULLIF|NUMERIC|OFF?|OFFSETS?|ON|OPEN(?:DATASOURCE|QUERY|ROWSET)?|OPTIMIZE|OPTION(?:AL(?:LY)?)?|ORDER|OUT(?:ER|FILE)?|OVER|PARTIAL|PARTITION|PERCENT|PIVOT|PLAN|POINT|POLYGON|PRECEDING|PRECISION|PREPARE|PREV|PRIMARY|PRINT|PRIVILEGES|PROC(?:EDURE)?|PUBLIC|PURGE|QUICK|RAISERROR|READS?|REAL|RECONFIGURE|REFERENCES|RELEASE|RENAME|REPEAT(?:ABLE)?|REPLACE|REPLICATION|REQUIRE|RESIGNAL|RESTORE|RESTRICT|RETURNS?|REVOKE|RIGHT|ROLLBACK|ROUTINE|ROW(?:COUNT|GUIDCOL|S)?|RTREE|RULE|SAVE(?:POINT)?|SCHEMA|SECOND|SELECT|SERIAL(?:IZABLE)?|SESSION(?:_USER)?|SET(?:USER)?|SHARE|SHOW|SHUTDOWN|SIMPLE|SMALLINT|SNAPSHOT|SOME|SONAME|SQL|START(?:ING)?|STATISTICS|STATUS|STRIPED|SYSTEM_USER|TABLES?|TABLESPACE|TEMP(?:ORARY|TABLE)?|TERMINATED|TEXT(?:SIZE)?|THEN|TIME(?:STAMP)?|TINY(?:BLOB|INT|TEXT)|TOP?|TRAN(?:SACTIONS?)?|TRIGGER|TRUNCATE|TSEQUAL|TYPES?|UNBOUNDED|UNCOMMITTED|UNDEFINED|UNION|UNIQUE|UNLOCK|UNPIVOT|UNSIGNED|UPDATE(?:TEXT)?|USAGE|USE|USER|USING|VALUES?|VAR(?:BINARY|CHAR|CHARACTER|YING)|VIEW|WAITFOR|WARNINGS|WHEN|WHERE|WHILE|WITH(?: ROLLUP|IN)?|WORK|WRITE(?:TEXT)?|YEAR)\b/i,
        'boolean': /\b(?:TRUE|FALSE|NULL)\b/i,
        'number': /\b0x[\da-f]+\b|\b\d+\.?\d*|\B\.\d+\b/i,
        'operator': /[-+*\/=%^~]|&&?|\|\|?|!=?|<(?:=>?|<|>)?|>[>=]?|\b(?:AND|BETWEEN|IN|LIKE|NOT|OR|IS|DIV|REGEXP|RLIKE|SOUNDS LIKE|XOR)\b/i,
        'punctuation': /[;[\]()`,.]/
    };

    Prism.languages.json = {
        'property': {
            pattern: /"(?:\\.|[^\\"\r\n])*"(?=\s*:)/,
            greedy: true
        },
        'string': {
            pattern: /"(?:\\.|[^\\"\r\n])*"(?!\s*:)/,
            greedy: true
        },
        'number': /\b-?(0x[\dA-Fa-f]+|\d*\.?\d+([eE][+-]?\d+)?)\b/,
        'boolean': /\b(?:true|false)\b/,
        'null': /\bnull\b/,
        'punctuation': /[{}[\]);,]/,
        'operator': /:/g
    };

    Prism.languages.turtle = {
        'comment': {
            pattern: /(^|[^\\<])(?:\/\*[\s\S]*?\*\/|(?:--|\/\/|#)(?![^\n<>]*>).*?)/,
            lookbehind: true
        },
        'string': {
            pattern: /(^|[^@@\\])("|')(?:\\[\s\S]|(?!\2)[^\\])*\2/,
            greedy: true,
            lookbehind: true
        },
        'uri': {
            pattern: /(^|[^@@\\])(<[^>\n]*>)/,
            greedy: true,
            lookbehind: true
        },
        'suri': {
            pattern: /(^|[^@@\\])\b[a-zA-Z_][a-zA-Z0-9_.-]*:[a-zA-Z0-9_.-]+\b/,
            lookbehind: true
        },
        'variable': /\?[\w.$]+|\?(["'`])(?:\\[\s\S]|(?!\1)[^\\])+\1/,
        'keyword': /\b(?:a|@prefix|@base|BASE|PREFIX|SELECT|CONSTRUCT|DESCRIBE|ASK|WHERE|ORDER BY|ASC|DESC|LIMIT|OFFSET|FILTER|OPTIONAL|GRAPH|UNION|DISTINCT|REDUCED|FROM|NAMED|GROUP BY|HAVING)\b/i,
        'boolean': /\b(?:true|false)\b/i,
        'number': /\b0x[\da-f]+\b|\b\d+\.?\d*|\B\.\d+\b/i,
        'operator': /[-+*\/=%^~]|&&?|\|\|?|!=?|<(?:=>?|<|>)?|>[>=]?|\b(?:AND|IN|NOT|OR|IS)\b/i,
        'punctuation': /[;[\]()`,.]/
    };
}

function onInput(textAreaValue, textAreaElement, highlightElement, codeElement) {
    const value = textAreaValue;
    textAreaElement.value = value;
    this.update(value, codeElement);
    this.syncScroll(textAreaElement, highlightElement);
}

function checkTab(element, key) {
    let code = element.value;
    if (key === "Tab") {
        let before_tab = code.slice(0, element.selectionStart);
        let after_tab = code.slice(element.selectionEnd, element.value.length);
        let cursor_pos = element.selectionStart + 1;
        element.value = before_tab + "\t" + after_tab;
        element.selectionStart = cursor_pos;
        element.selectionEnd = cursor_pos;
        this.update(element.value);
    }
}

function update(text, codeElement) {
    text = text || "";

    if (codeElement != undefined) {
        // Handle final newlines (see article)
        if (text[text.length - 1] === "\n") {
            text += " ";
        }
        // Update code
        codeElement.innerHTML = text.replace(/&/g, "&amp;").replace(/</g, "&lt;");
        // Syntax Highlight
        Prism.highlightElement(codeElement);
    }
}

function syncScroll(codeElement, result_element) {
    result_element.scrollTop = codeElement.scrollTop;
    result_element.scrollLeft = codeElement.scrollLeft;
}