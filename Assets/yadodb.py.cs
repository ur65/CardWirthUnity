
using os;

using sqlite3;

using threading;

using time;

using cw;

using synclock = cw.util.synclock;

using System.Collections.Generic;

public static class yadodb {
    
    public static object _lock = threading.Lock();
    
    public static object YADO = 0;
    
    public static object PARTY = 1;
    
    // カードのデータベース。ロックのタイムアウトは30秒指定。
    public class YadoDB
        : object {
        
        [synclock(_lock)]
        public YadoDB(object ypath, object mode = YADO) {
            object haspostype;
            object s;
            object fname;
            this.ypath = ypath;
            if (mode == YADO) {
                fname = "Yado.db";
            } else {
                fname = "Card.db";
            }
            this.name = os.path.join(ypath, fname);
            this.mode = mode;
            if (os.path.isfile(this.name)) {
                this.con = sqlite3.connect(this.name, timeout: 30000);
                this.con.row_factory = sqlite3.Row;
                this.cur = this.con.cursor();
                var reqcommit = false;
                // cardorderテーブルが存在しない場合は作成する(旧バージョンとの互換性維持)
                var cur = this.con.execute("PRAGMA table_info('cardorder')");
                var res = cur.fetchall();
                if (!res) {
                    s = @"
                    CREATE TABLE cardorder (
                        fpath TEXT,
                        numorder INTEGER,
                        PRIMARY KEY (fpath)
                    )
                ";
                    this.cur.execute(s);
                }
                // cardimageテーブルが存在しない場合は作成する(0.12.3以前との互換性維持)
                cur = this.con.execute("PRAGMA table_info('cardimage')");
                res = cur.fetchall();
                if (!res) {
                    s = @"
                    CREATE TABLE cardimage (
                        fpath TEXT,
                        numorder INTEGER,
                        imgpath TEXT,
                        postype TEXT,
                        PRIMARY KEY (fpath, numorder)
                    )
                ";
                    this.cur.execute(s);
                } else {
                    // postype列が存在しない場合は作成する(～1.1との互換性維持)
                    cur = this.con.execute("PRAGMA table_info('cardimage')");
                    res = cur.fetchall();
                    haspostype = false;
                    foreach (var rec in res) {
                        if (rec[1] == "postype") {
                            haspostype = true;
                            break;
                        }
                    }
                    if (!haspostype) {
                        // 値はNone(Default扱い)
                        this.cur.execute("ALTER TABLE cardimage ADD COLUMN postype TEXT");
                        reqcommit = true;
                    }
                }
                if (this.mode == YADO) {
                    // adventurerorderテーブルが存在しない場合は作成する(旧バージョンとの互換性維持)
                    cur = this.con.execute("PRAGMA table_info('adventurerorder')");
                    res = cur.fetchall();
                    if (!res) {
                        s = @"
                        CREATE TABLE adventurerorder (
                            fpath TEXT,
                            numorder INTEGER,
                            PRIMARY KEY (fpath)
                        )
                    ";
                        this.cur.execute(s);
                    }
                    // adventurerorderテーブルが存在しない場合は作成する(0.12.3以前との互換性維持)
                    cur = this.con.execute("PRAGMA table_info('adventurerimage')");
                    res = cur.fetchall();
                    if (!res) {
                        s = @"
                        CREATE TABLE adventurerimage (
                            fpath TEXT,
                            numorder INTEGER,
                            imgpath TEXT,
                            postype TEXT,
                            PRIMARY KEY (fpath, numorder)
                        )
                    ";
                        this.cur.execute(s);
                    } else {
                        // postype列が存在しない場合は作成する(～1.1との互換性維持)
                        cur = this.con.execute("PRAGMA table_info('adventurerimage')");
                        res = cur.fetchall();
                        haspostype = false;
                        foreach (var rec in res) {
                            if (rec[1] == "postype") {
                                haspostype = true;
                                break;
                            }
                        }
                        if (!haspostype) {
                            // 値はNone(Default扱い)
                            this.cur.execute("ALTER TABLE adventurerimage ADD COLUMN postype TEXT");
                            reqcommit = true;
                        }
                    }
                    // partyorderテーブルが存在しない場合は作成する(旧バージョンとの互換性維持)
                    cur = this.con.execute("PRAGMA table_info('partyorder')");
                    res = cur.fetchall();
                    if (!res) {
                        s = @"
                        CREATE TABLE partyorder (
                            fpath TEXT,
                            numorder INTEGER,
                            PRIMARY KEY (fpath)
                        )
                    ";
                        this.cur.execute(s);
                    }
                }
                // moved列,scenariocard列,versionhint列,star列が存在しない
                // 場合は作成する(旧バージョンとの互換性維持)
                cur = this.con.execute("PRAGMA table_info('card')");
                res = cur.fetchall();
                var hasmoved = false;
                var hasscenariocard = false;
                var hasversionhint = false;
                var haswsnversion = false;
                var hasstar = false;
                foreach (var rec in res) {
                    if (rec[1] == "moved") {
                        hasmoved = true;
                    } else if (rec[1] == "scenariocard") {
                        hasscenariocard = true;
                    } else if (rec[1] == "versionhint") {
                        hasversionhint = true;
                    } else if (rec[1] == "wsnversion") {
                        haswsnversion = true;
                    } else if (rec[1] == "star") {
                        hasstar = true;
                    }
                    if (all(Tuple.Create(hasmoved, hasscenariocard, hasversionhint, haswsnversion, hasstar))) {
                        break;
                    }
                }
                if (!hasmoved) {
                    this.cur.execute("ALTER TABLE card ADD COLUMN moved INTEGER");
                    this.cur.execute("UPDATE card SET moved=?", Tuple.Create(0));
                    reqcommit = true;
                }
                if (!hasscenariocard) {
                    this.cur.execute("ALTER TABLE card ADD COLUMN scenariocard INTEGER");
                    this.cur.execute("UPDATE card SET scenariocard=?", Tuple.Create(0));
                    reqcommit = true;
                }
                if (!hasversionhint) {
                    this.cur.execute("ALTER TABLE card ADD COLUMN versionhint TEXT");
                    this.cur.execute("UPDATE card SET versionhint=?", Tuple.Create(""));
                    reqcommit = true;
                }
                if (!hasstar) {
                    this.cur.execute("ALTER TABLE card ADD COLUMN star INTEGER");
                    this.cur.execute("UPDATE card SET star=?", Tuple.Create(0));
                    reqcommit = true;
                }
                if (!haswsnversion) {
                    // 値はNoneのままにしておく
                    this.cur.execute("ALTER TABLE card ADD COLUMN wsnversion TEXT");
                    reqcommit = true;
                }
                if (this.mode == YADO) {
                    // desc, versionhint, wsnversion列が存在しない場合は作成する
                    // (旧バージョンとの互換性維持)
                    cur = this.con.execute("PRAGMA table_info('adventurer')");
                    res = cur.fetchall();
                    var hasdesc = false;
                    hasversionhint = false;
                    haswsnversion = false;
                    foreach (var rec in res) {
                        if (rec[1] == "desc") {
                            hasdesc = true;
                        } else if (rec[1] == "versionhint") {
                            hasversionhint = true;
                        } else if (rec[1] == "wsnversion") {
                            haswsnversion = true;
                        }
                        if (all(Tuple.Create(hasdesc, hasversionhint, haswsnversion))) {
                            break;
                        }
                    }
                    if (!hasdesc) {
                        this.cur.execute("ALTER TABLE adventurer ADD COLUMN desc TEXT");
                        this.cur.execute("UPDATE adventurer SET mtime=?", Tuple.Create(0));
                        reqcommit = true;
                    }
                    if (!hasversionhint) {
                        this.cur.execute("ALTER TABLE adventurer ADD COLUMN versionhint TEXT");
                        this.cur.execute("UPDATE adventurer SET versionhint=?", Tuple.Create(""));
                        reqcommit = true;
                    }
                    if (!haswsnversion) {
                        // 値はNoneのままにしておく
                        this.cur.execute("ALTER TABLE adventurer ADD COLUMN wsnversion TEXT");
                        reqcommit = true;
                    }
                }
                if (this.mode == YADO) {
                    // partyrecordテーブルが存在しない場合は作成する(旧バージョンとの互換性維持)
                    cur = this.con.execute("PRAGMA table_info('partyrecord')");
                    res = cur.fetchall();
                    if (!res) {
                        s = @"
                        CREATE TABLE partyrecord (
                            fpath TEXT,
                            name TEXT,
                            money INTEGER,
                            members TEXT,
                            membernames TEXT,
                            backpack TEXT,
                            ctime INTEGER,
                            mtime INTEGER,
                            PRIMARY KEY (fpath)
                        )
                    ";
                        this.cur.execute(s);
                        reqcommit = true;
                    }
                }
                if (this.mode == YADO) {
                    // membernames列が存在しない場合は作成する
                    // (旧バージョンとの互換性維持)
                    cur = this.con.execute("PRAGMA table_info('partyrecord')");
                    res = cur.fetchall();
                    var hasmembernames = false;
                    foreach (var rec in res) {
                        if (rec[1] == "membernames") {
                            hasmembernames = true;
                            break;
                        }
                    }
                    if (!hasmembernames) {
                        this.cur.execute("ALTER TABLE partyrecord ADD COLUMN membernames TEXT");
                        this.cur.execute("UPDATE partyrecord SET membernames=?", Tuple.Create(""));
                        reqcommit = true;
                    }
                }
                if (this.mode == YADO) {
                    // savedjpdcimageテーブルが存在しない場合は作成する
                    // (旧バージョンとの互換性維持)
                    cur = this.con.execute("PRAGMA table_info('savedjpdcimage')");
                    res = cur.fetchall();
                    if (!res) {
                        s = @"
                        CREATE TABLE savedjpdcimage (
                            fpath TEXT,
                            scenarioname TEXT,
                            scenarioauthor TEXT,
                            dpath TEXT,
                            fpaths TEXT,
                            ctime INTEGER,
                            mtime INTEGER,
                            PRIMARY KEY (fpath)
                        )
                    ";
                        this.cur.execute(s);
                    }
                }
                if (reqcommit) {
                    this.con.commit();
                }
            } else {
                var dname = os.path.dirname(this.name);
                if (!os.path.isdir(dname)) {
                    os.makedirs(dname);
                }
                this.con = sqlite3.connect(this.name, timeout: 30000);
                this.con.row_factory = sqlite3.Row;
                this.cur = this.con.cursor();
                // テーブル作成
                // カード置場のカード
                s = @"
                CREATE TABLE card (
                    fpath TEXT,
                    type INTEGER,
                    id INTEGER,
                    name TEXT,
                    imgpath TEXT,
                    desc TEXT,
                    scenario TEXT,
                    author TEXT,
                    keycodes TEXT,
                    uselimit INTEGER,
                    target TEXT,
                    allrange INTEGER,
                    premium TEXT,
                    physical TEXT,
                    mental TEXT,
                    level INTEGER,
                    maxuselimit INTEGER,
                    price INTEGER,
                    hold INTEGER,
                    enhance_avo INTEGER,
                    enhance_res INTEGER,
                    enhance_def INTEGER,
                    enhance_avo_used INTEGER,
                    enhance_res_used INTEGER,
                    enhance_def_used INTEGER,
                    attachment INTEGER,
                    moved INTEGER,
                    scenariocard INTEGER,
                    versionhint TEXT,
                    wsnversion TEXT,
                    star INTEGER,
                    ctime INTEGER,
                    mtime INTEGER,
                    PRIMARY KEY (fpath)
                )
            ";
                this.cur.execute(s);
                // カードイメージ(複数あるもの)
                s = @"
                CREATE TABLE cardimage (
                    fpath TEXT,
                    numorder INTEGER,
                    imgpath TEXT,
                    postype TEXT,
                    PRIMARY KEY (fpath, numorder)
                )
            ";
                this.cur.execute(s);
                // カードの並び順
                s = @"
                CREATE TABLE cardorder (
                    fpath TEXT,
                    numorder INTEGER,
                    PRIMARY KEY (fpath)
                )
            ";
                this.cur.execute(s);
                if (this.mode == YADO) {
                    // 宿帳とアルバムの冒険者
                    s = @"
                    CREATE TABLE adventurer (
                        fpath TEXT,
                        level INTEGER,
                        name TEXT,
                        desc TEXT,
                        imgpath TEXT,
                        album INTEGER,
                        lost INTEGER,
                        sex TEXT,
                        age TEXT,
                        ep INTEGER,
                        leavenoalbum INTEGER,
                        gene TEXT,
                        history TEXT,
                        race TEXT,
                        versionhint TEXT,
                        wsnversion TEXT,
                        ctime INTEGER,
                        mtime INTEGER,
                        PRIMARY KEY (fpath)
                    )
                ";
                    this.cur.execute(s);
                    // 冒険者のイメージ(複数あるもの)
                    s = @"
                    CREATE TABLE adventurerimage (
                        fpath TEXT,
                        numorder INTEGER,
                        imgpath TEXT,
                        postype TEXT,
                        PRIMARY KEY (fpath, numorder)
                    )
                ";
                    this.cur.execute(s);
                    // 宿帳の並び順
                    s = @"
                    CREATE TABLE adventurerorder (
                        fpath TEXT,
                        numorder INTEGER,
                        PRIMARY KEY (fpath)
                    )
                ";
                    this.cur.execute(s);
                    // パーティ
                    s = @"
                    CREATE TABLE party (
                        fpath TEXT,
                        name TEXT,
                        money INTEGER,
                        members TEXT,
                        ctime INTEGER,
                        mtime INTEGER,
                        PRIMARY KEY (fpath)
                    )
                ";
                    this.cur.execute(s);
                    // パーティの並び順
                    s = @"
                    CREATE TABLE partyorder (
                        fpath TEXT,
                        numorder INTEGER,
                        PRIMARY KEY (fpath)
                    )
                ";
                    this.cur.execute(s);
                    // パーティ記録
                    s = @"
                    CREATE TABLE partyrecord (
                        fpath TEXT,
                        name TEXT,
                        money INTEGER,
                        members TEXT,
                        membernames TEXT,
                        backpack TEXT,
                        ctime INTEGER,
                        mtime INTEGER,
                        PRIMARY KEY (fpath)
                    )
                ";
                    this.cur.execute(s);
                    // 保存されたJPDCイメージ
                    s = @"
                    CREATE TABLE savedjpdcimage (
                        fpath TEXT,
                        scenarioname TEXT,
                        scenarioauthor TEXT,
                        dpath TEXT,
                        fpaths TEXT,
                        ctime INTEGER,
                        mtime INTEGER,
                        PRIMARY KEY (fpath)
                    )
                ";
                    this.cur.execute(s);
                }
            }
        }
        
        // データベースを更新する。
        [synclock(_lock)]
        public virtual object update(
            object cards = true,
            object adventurers = true,
            object parties = true,
            object cardorder = new Dictionary<object, object> {
            }.copy(),
            object adventurerorder = new Dictionary<object, object> {
            }.copy(),
            object partyorder = new Dictionary<object, object> {
            }.copy(),
            object partyrecord = true,
            object savedjpdcimage = true) {
            object orderc;
            object fpath;
            object path;
            object dbpaths;
            object data;
            object s;
            Func<object, object, object, object, object, object, object> walk = (dpath,headertable,xmlname,insert,insertheader,args) => {
                object path;
                var dname = cw.util.join_paths(this.ypath, dpath);
                if (os.path.isdir(dname)) {
                    foreach (var fname in os.listdir(dname)) {
                        if (xmlname) {
                            path = cw.util.join_paths(dpath, fname, xmlname);
                            if (!os.path.isfile(cw.util.join_paths(this.ypath, path))) {
                                continue;
                            }
                        } else {
                            if (!fname.lower().endswith(".xml")) {
                                continue;
                            }
                            path = cw.util.join_paths(dpath, fname);
                        }
                        if (!dbpaths.Contains(path)) {
                            if (headertable is dict && headertable.Contains(path)) {
                                insertheader(headertable[path], args);
                            } else {
                                insert(cw.util.join_paths(this.ypath, path), args);
                            }
                        }
                    }
                }
            };
            if (cards) {
                s = "SELECT fpath, mtime FROM card";
                this.cur.execute(s);
                data = this.cur.fetchall();
                dbpaths = new HashSet<object>();
                foreach (var t in data) {
                    path = cw.util.join_paths(this.ypath, t[0]);
                    if (!os.path.isfile(path)) {
                        this._delete_card(t[0], false);
                    } else {
                        dbpaths.add(t[0]);
                        if (os.path.getmtime(path) > t[1]) {
                            // 情報を更新
                            if (cards is dict && cards.Contains(path)) {
                                this._insert_cardheader(cards[path], false);
                            } else {
                                this._insert_card(path, false);
                            }
                        }
                    }
                }
                walk("SkillCard", cards, "", this._insert_card, this._insert_cardheader, false);
                walk("ItemCard", cards, "", this._insert_card, this._insert_cardheader, false);
                walk("BeastCard", cards, "", this._insert_card, this._insert_cardheader, false);
                if (cardorder) {
                    // カードの並び順を登録する
                    s = "DELETE FROM cardorder";
                    this.cur.execute(s);
                    foreach (var _tup_1 in cardorder.items()) {
                        fpath = _tup_1.Item1;
                        orderc = _tup_1.Item2;
                        s = @"
                        INSERT OR REPLACE INTO cardorder VALUES(
                            ?,
                            ?
                        )
                    ";
                        this.cur.execute(s, Tuple.Create(fpath, orderc));
                    }
                }
            }
            if (this.mode == YADO && (adventurers is dict || adventurers)) {
                s = "SELECT fpath, mtime, album FROM adventurer";
                this.cur.execute(s);
                data = this.cur.fetchall();
                dbpaths = new HashSet<object>();
                foreach (var t in data) {
                    path = cw.util.join_paths(this.ypath, t[0]);
                    if (!os.path.isfile(path)) {
                        this._delete_adventurer(t[0], false);
                    } else {
                        dbpaths.add(t[0]);
                        if (os.path.getmtime(path) > t[1]) {
                            // 情報を更新
                            if (adventurers is dict && adventurers.Contains(path)) {
                                this._insert_adventurerheader(adventurers[path], @bool(t[2]), false);
                            } else {
                                this._insert_adventurer(path, @bool(t[2]), false);
                            }
                        }
                    }
                }
                walk("Adventurer", adventurers, "", this._insert_adventurer, this._insert_adventurerheader, false, false);
                walk("Album", new Dictionary<object, object> {
                }, "", this._insert_adventurer, this._insert_adventurerheader, true, false);
                if (adventurerorder) {
                    // 冒険者の並び順を登録する
                    s = "DELETE FROM adventurerorder";
                    this.cur.execute(s);
                    foreach (var _tup_2 in adventurerorder.items()) {
                        fpath = _tup_2.Item1;
                        orderc = _tup_2.Item2;
                        s = @"
                        INSERT OR REPLACE INTO adventurerorder VALUES(
                            ?,
                            ?
                        )
                    ";
                        this.cur.execute(s, Tuple.Create(fpath, orderc));
                    }
                }
            }
            if (this.mode == YADO && parties) {
                s = "SELECT fpath, mtime FROM party";
                this.cur.execute(s);
                data = this.cur.fetchall();
                dbpaths = new HashSet<object>();
                foreach (var t in data) {
                    path = cw.util.join_paths(this.ypath, t[0]);
                    if (!os.path.isfile(path)) {
                        this._delete_party(t[0], false);
                    } else {
                        dbpaths.add(t[0]);
                        if (os.path.getmtime(path) > t[1]) {
                            // 情報を更新
                            if (parties is dict && parties.Contains(path)) {
                                this._insert_partyheader(parties[path], false);
                            } else {
                                this._insert_party(path, false);
                            }
                        }
                    }
                }
                foreach (var dpath in os.listdir(cw.util.join_paths(this.ypath, "Party"))) {
                    walk(cw.util.join_paths("Party", dpath), parties, "", this._insert_party, this._insert_partyheader, false);
                }
                if (partyorder) {
                    // 冒険者の並び順を登録する
                    s = "DELETE FROM partyorder";
                    this.cur.execute(s);
                    foreach (var _tup_3 in partyorder.items()) {
                        fpath = _tup_3.Item1;
                        orderc = _tup_3.Item2;
                        s = @"
                        INSERT OR REPLACE INTO partyorder VALUES(
                            ?,
                            ?
                        )
                    ";
                        this.cur.execute(s, Tuple.Create(fpath, orderc));
                    }
                }
            }
            if (this.mode == YADO && partyrecord) {
                s = "SELECT fpath, mtime FROM partyrecord";
                this.cur.execute(s);
                data = this.cur.fetchall();
                dbpaths = new HashSet<object>();
                foreach (var t in data) {
                    path = cw.util.join_paths(this.ypath, t[0]);
                    if (!os.path.isfile(path)) {
                        this._delete_partyrecord(t[0], false);
                    } else {
                        dbpaths.add(t[0]);
                        if (os.path.getmtime(path) > t[1]) {
                            // 情報を更新
                            if (partyrecord is dict && partyrecord.Contains(t[0])) {
                                this._insert_partyrecordheader(partyrecord[t[0]], false);
                            } else {
                                this._insert_partyrecord(path, false);
                            }
                        }
                    }
                }
                walk("PartyRecord", partyrecord, "", this._insert_partyrecord, this._insert_partyrecordheader, false);
            }
            if (this.mode == YADO && savedjpdcimage) {
                s = "SELECT fpath, mtime FROM savedjpdcimage";
                this.cur.execute(s);
                data = this.cur.fetchall();
                dbpaths = new HashSet<object>();
                foreach (var t in data) {
                    path = cw.util.join_paths(this.ypath, t[0]);
                    if (!os.path.isfile(path)) {
                        this._delete_savedjpdcimage(t[0], false);
                    } else {
                        dbpaths.add(t[0]);
                        if (os.path.getmtime(path) > t[1]) {
                            // 情報を更新
                            if (savedjpdcimage is dict && savedjpdcimage.Contains(t[0])) {
                                this._insert_savedjpdcimageheader(savedjpdcimage[t[0]], false);
                            } else {
                                this._insert_savedjpdcimage(path, false);
                            }
                        }
                    }
                }
                walk("SavedJPDCImage", savedjpdcimage, "SavedJPDCImage.xml", this._insert_savedjpdcimage, this._insert_savedjpdcimageheader, false);
            }
            this.con.commit();
        }
        
        // 肥大化したDBファイルのサイズを最適化する。
        public virtual object vacuum(object commit = true) {
            var s = "VACUUM card";
            this.cur.execute(s);
            if (this.mode == YADO) {
                s = "VACUUM adventurer";
                this.cur.execute(s);
                s = "VACUUM party";
                this.cur.execute(s);
                s = "VACUUM partyrecord";
                this.cur.execute(s);
                s = "VACUUM savedjpdcimage";
                this.cur.execute(s);
            }
            if (commit) {
                this.con.commit();
            }
        }
        
        public virtual object _delete_card(object path, object commit = true) {
            var s = "DELETE FROM card WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            s = "DELETE FROM cardimage WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            s = "DELETE FROM cardorder WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            if (commit) {
                this.con.commit();
            }
        }
        
        public virtual object _delete_adventurer(object path, object commit = true) {
            var s = "DELETE FROM adventurer WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            s = "DELETE FROM adventurerimage WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            s = "DELETE FROM adventurerorder WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            if (commit) {
                this.con.commit();
            }
        }
        
        public virtual object _delete_party(object path, object commit = true) {
            var s = "DELETE FROM party WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            s = "DELETE FROM partyorder WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            if (commit) {
                this.con.commit();
            }
        }
        
        public virtual object _delete_partyrecord(object path, object commit = true) {
            var s = "DELETE FROM partyrecord WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            if (commit) {
                this.con.commit();
            }
        }
        
        public virtual object _delete_savedjpdcimage(object path, object commit = true) {
            var s = "DELETE FROM savedjpdcimage WHERE fpath=?";
            this.cur.execute(s, Tuple.Create(path));
            if (commit) {
                this.con.commit();
            }
        }
        
        [synclock(_lock)]
        public virtual object delete_savedjpdcimage(object path, object commit = true) {
            this._delete_savedjpdcimage(path, commit);
        }
        
        [synclock(_lock)]
        public virtual object insert_cardheader(object header, object commit = true, object cardorder = -1) {
            return this._insert_cardheader(header, commit, cardorder);
        }
        
        // データベースにカードを登録する。
        public virtual object _insert_cardheader(object header, object commit = true, object cardorder = -1) {
            object postype;
            object imgpath;
            var s = @"
        INSERT OR REPLACE INTO card(
            fpath,
            type,
            id,
            name,
            imgpath,
            desc,
            scenario,
            author,
            keycodes,
            uselimit,
            target,
            allrange,
            premium,
            physical,
            mental,
            level,
            maxuselimit,
            price,
            hold,
            enhance_avo,
            enhance_res,
            enhance_def,
            enhance_avo_used,
            enhance_res_used,
            enhance_def_used,
            attachment,
            moved,
            scenariocard,
            versionhint,
            wsnversion,
            star,
            ctime,
            mtime
        ) VALUES(
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?
        )
        ";
            var fpath = cw.util.relpath(header.fpath, this.ypath);
            fpath = cw.util.join_paths(fpath);
            var ctime = time.time();
            var mtime = os.path.getmtime(header.fpath);
            if (header.imgpaths.Count == 1 && header.imgpaths[0].postype == "Default") {
                imgpath = header.imgpaths[0].path;
            } else if (!header.imgpaths) {
                imgpath = "";
            } else {
                imgpath = null;
            }
            this.cur.execute(s, Tuple.Create(fpath, header.type, header.id, header.name, imgpath, header.desc, header.scenario, header.author, "\n".join(header.keycodes[:: - 1]), header.uselimit, header.target, header.allrange, header.premium, header.physical, header.mental, header.level, header.maxuselimit, header.price, header.hold, header.enhance_avo, header.enhance_res, header.enhance_def, header.enhance_avo_used, header.enhance_res_used, header.enhance_def_used, header.attachment, header.moved, header.scenariocard ? 1 : 0, cw.cwpy.sct.to_basehint(header.versionhint), header.wsnversion, header.star, ctime, mtime));
            if (-1 < cardorder) {
                s = @"
            INSERT OR REPLACE INTO cardorder VALUES(
                ?,
                ?
            )
            ";
                this.cur.execute(s, Tuple.Create(fpath, cardorder));
            }
            if (header.imgpaths && !(header.imgpaths.Count == 1 && header.imgpaths[0].postype == "Default")) {
                s = @"
            DELETE FROM cardimage WHERE fpath=?
            ";
                this.cur.execute(s, Tuple.Create(fpath));
                foreach (var _tup_1 in header.imgpaths.Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                    var i = _tup_1.Item1;
                    imgpath = _tup_1.Item2;
                    s = @"
                INSERT OR REPLACE INTO cardimage (
                    fpath,
                    numorder,
                    imgpath,
                    postype
                ) VALUES (
                    ?,
                    ?,
                    ?,
                    ?
                )
                ";
                    if (imgpath.postype == "Default") {
                        postype = null;
                    } else {
                        postype = imgpath.postype;
                    }
                    this.cur.execute(s, Tuple.Create(fpath, i, imgpath.path, postype));
                }
            }
            if (commit) {
                this.con.commit();
            }
        }
        
        [synclock(_lock)]
        public virtual object insert_card(object path, object commit = true, object cardorder = -1) {
            return this._insert_card(path, commit, cardorder);
        }
        
        public virtual object _insert_card(object path, object commit = true, object cardorder = -1) {
            try {
                var data = cw.data.xml2element(path);
                var header = cw.header.CardHeader(carddata: data);
                header.fpath = path;
                return this._insert_cardheader(header, commit, cardorder);
            } catch (Exception) {
                cw.util.print_ex();
            }
        }
        
        public virtual object get_cards() {
            object imgdbrec;
            object owner;
            var s = @"
            SELECT
                card.fpath,
                type,
                id,
                name,
                imgpath,
                desc,
                scenario,
                author,
                keycodes,
                uselimit,
                target,
                allrange,
                premium,
                physical,
                mental,
                level,
                maxuselimit,
                price,
                hold,
                enhance_avo,
                enhance_res,
                enhance_def,
                enhance_avo_used,
                enhance_res_used,
                enhance_def_used,
                attachment,
                moved,
                scenariocard,
                versionhint,
                wsnversion,
                star,
                ctime,
                mtime,
                numorder
            FROM
                card
                LEFT OUTER JOIN
                    cardorder
                ON
                    card.fpath = cardorder.fpath
            ORDER BY
                numorder,
                name
        ";
            this.cur.execute(s);
            var headers = new List<object>();
            if (this.mode == YADO) {
                owner = "STOREHOUSE";
            } else {
                owner = "BACKPACK";
            }
            s = @"
            SELECT
                imgpath,
                postype
            FROM
                cardimage
            WHERE
                fpath = ?
            ORDER BY
                numorder
        ";
            var recs = this.cur.fetchall();
            foreach (var _tup_1 in recs.Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                var order = _tup_1.Item1;
                var rec = _tup_1.Item2;
                if (rec["imgpath"] == null) {
                    imgdbrec = this.cur.execute(s, Tuple.Create(rec["fpath"]));
                } else {
                    imgdbrec = null;
                }
                var header = cw.header.CardHeader(dbrec: rec, imgdbrec: imgdbrec, dbowner: owner);
                header.order = order;
                header.fpath = cw.util.join_paths(this.ypath, header.fpath);
                headers.append(header);
            }
            return headers;
        }
        
        [synclock(_lock)]
        public virtual object get_cardfpaths(object scenariocard = true) {
            var s = @"
            SELECT
                card.fpath
            FROM
                card
                LEFT OUTER JOIN
                    cardorder
                ON
                    card.fpath = cardorder.fpath
            WHERE
                scenariocard=?
            ORDER BY
                numorder,
                name
        ";
            this.cur.execute(s, Tuple.Create(scenariocard ? 1 : 0));
            var seq = new List<object>();
            foreach (var rec in this.cur) {
                seq.append(rec["fpath"]);
            }
            return seq;
        }
        
        [synclock(_lock)]
        public virtual object insert_adventurerheader(object header, object commit = true, object adventurerorder = -1) {
            return this._insert_adventurerheader(header, commit, adventurerorder);
        }
        
        // データベースに冒険者を登録する。
        public virtual object _insert_adventurerheader(object header, object commit = true, object adventurerorder = -1) {
            object postype;
            object album;
            object imgpath;
            var s = @"
        INSERT OR REPLACE INTO adventurer(
            fpath,
            level,
            name,
            desc,
            imgpath,
            album,
            lost,
            sex,
            age,
            ep,
            leavenoalbum,
            gene,
            history,
            race,
            versionhint,
            wsnversion,
            ctime,
            mtime
        ) VALUES(
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?
        )
        ";
            var fpath = cw.util.relpath(header.fpath, this.ypath);
            fpath = cw.util.join_paths(fpath);
            var ctime = time.time();
            var mtime = os.path.getmtime(header.fpath);
            if (header.imgpaths.Count == 1 && header.imgpaths[0].postype == "Default") {
                imgpath = header.imgpaths[0].path;
            } else if (!header.imgpaths) {
                imgpath = "";
            } else {
                imgpath = null;
            }
            if (header.album) {
                album = 1;
            } else {
                album = 0;
            }
            this.cur.execute(s, Tuple.Create(fpath, header.level, header.name, header.desc, imgpath, album, header.lost, header.sex, header.age, header.ep, header.leavenoalbum, header.gene.get_str(), "\n".join(header.history), header.race, cw.cwpy.sct.to_basehint(header.versionhint), header.wsnversion, ctime, mtime));
            if (-1 < adventurerorder) {
                s = @"
            INSERT OR REPLACE INTO adventurerorder VALUES(
                ?,
                ?
            )
            ";
                this.cur.execute(s, Tuple.Create(fpath, adventurerorder));
            }
            if (header.imgpaths && !(header.imgpaths.Count == 1 && header.imgpaths[0].postype == "Default")) {
                s = @"
            DELETE FROM adventurerimage WHERE fpath=?
            ";
                this.cur.execute(s, Tuple.Create(fpath));
                foreach (var _tup_1 in header.imgpaths.Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                    var i = _tup_1.Item1;
                    imgpath = _tup_1.Item2;
                    s = @"
                INSERT OR REPLACE INTO adventurerimage (
                    fpath,
                    numorder,
                    imgpath,
                    postype
                ) VALUES (
                    ?,
                    ?,
                    ?,
                    ?
                )
                ";
                    if (imgpath.postype == "Default") {
                        postype = null;
                    } else {
                        postype = imgpath.postype;
                    }
                    this.cur.execute(s, Tuple.Create(fpath, i, imgpath.path, postype));
                }
            }
            if (commit) {
                this.con.commit();
            }
        }
        
        [synclock(_lock)]
        public virtual object insert_adventurer(object path, object album, object commit = true, object adventurerorder = -1) {
            return this._insert_adventurer(path, album, commit, adventurerorder);
        }
        
        public virtual object _insert_adventurer(object path, object album, object commit = true, object adventurerorder = -1) {
            try {
                var header = cw.header.AdventurerHeader(fpath: path, album: album);
                header.fpath = path;
                return this._insert_adventurerheader(header, commit, adventurerorder);
            } catch (Exception) {
                cw.util.print_ex();
            }
        }
        
        public virtual object get_adventurers(object album) {
            object imgdbrec;
            object s;
            if (album) {
                s = "SELECT * FROM adventurer WHERE album=? ORDER BY name";
                album = 1;
            } else {
                s = @"
            SELECT
                *
            FROM
                adventurer
                LEFT OUTER JOIN
                    adventurerorder
                ON
                    adventurer.fpath = adventurerorder.fpath
            WHERE
                lost=0 AND album=?
            ORDER BY
                numorder,
                name
            ";
                album = 0;
            }
            this.cur.execute(s, Tuple.Create(album));
            var headers = new List<object>();
            s = @"
            SELECT
                imgpath,
                postype
            FROM
                adventurerimage
            WHERE
                fpath = ?
            ORDER BY
                numorder
        ";
            var recs = this.cur.fetchall();
            foreach (var _tup_1 in recs.Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                var order = _tup_1.Item1;
                var rec = _tup_1.Item2;
                if (rec["imgpath"] == null) {
                    imgdbrec = this.cur.execute(s, Tuple.Create(rec["fpath"]));
                } else {
                    imgdbrec = null;
                }
                var header = cw.header.AdventurerHeader(dbrec: rec, imgdbrec: imgdbrec);
                header.order = order;
                header.fpath = cw.util.join_paths(this.ypath, header.fpath);
                headers.append(header);
            }
            return headers;
        }
        
        public virtual object get_standbys() {
            return this.get_adventurers(false);
        }
        
        public virtual object get_standbynames() {
            var s = "SELECT name FROM adventurer WHERE lost=0 AND album=? ORDER BY name";
            this.cur.execute(s, Tuple.Create(0));
            var names = new List<object>();
            foreach (var rec in this.cur) {
                names.append(rec[0]);
            }
            return names;
        }
        
        public virtual object get_album() {
            return this.get_adventurers(true);
        }
        
        [synclock(_lock)]
        public virtual object insert_partyheader(object header, object commit = true, object partyorder = -1) {
            return this._insert_partyheader(header, commit, partyorder);
        }
        
        // データベースにパーティを登録する。
        public virtual object _insert_partyheader(object header, object commit = true, object partyorder = -1) {
            var s = @"
        INSERT OR REPLACE INTO party(
            fpath,
            name,
            money,
            members,
            ctime,
            mtime
        ) VALUES(
            ?,
            ?,
            ?,
            ?,
            ?,
            ?
        )
        ";
            var fpath = cw.util.relpath(header.fpath, this.ypath);
            fpath = cw.util.join_paths(fpath);
            var ctime = time.time();
            var mtime = os.path.getmtime(header.fpath);
            this.cur.execute(s, Tuple.Create(fpath, header.name, header.money, "\n".join(header.members), ctime, mtime));
            if (-1 < partyorder) {
                s = @"
            INSERT OR REPLACE INTO partyorder VALUES(
                ?,
                ?
            )
            ";
                this.cur.execute(s, Tuple.Create(fpath, partyorder));
            }
            if (commit) {
                this.con.commit();
            }
        }
        
        [synclock(_lock)]
        public virtual object insert_party(object path, object commit = true, object partyorder = -1) {
            return this._insert_party(path, commit, partyorder);
        }
        
        public virtual object _insert_party(object path, object commit = true, object partyorder = -1) {
            try {
                // 新フォーマット(ディレクトリ)
                var data = cw.data.xml2etree(path);
                var e = data.find("Property");
                var header = cw.header.PartyHeader(e);
                header.fpath = path;
                return this._insert_partyheader(header, commit, partyorder);
            } catch (Exception) {
                cw.util.print_ex();
            }
        }
        
        public virtual object get_parties() {
            var s = @"
        SELECT
            *
        FROM
            party
            LEFT OUTER JOIN
                partyorder
            ON
                party.fpath = partyorder.fpath
        ORDER BY
            name
        ";
            this.cur.execute(s);
            var headers = new List<object>();
            foreach (var rec in this.cur) {
                var header = cw.header.PartyHeader(dbrec: rec);
                header.fpath = cw.util.join_paths(this.ypath, header.fpath);
                headers.append(header);
            }
            return headers;
        }
        
        [synclock(_lock)]
        public virtual object insert_partyrecordheader(object header, object commit = true) {
            return this._insert_partyrecordheader(header, commit);
        }
        
        // データベースにパーティ記録を登録する。
        public virtual object _insert_partyrecordheader(object header, object commit = true) {
            var s = @"
        INSERT OR REPLACE INTO partyrecord(
            fpath,
            name,
            money,
            members,
            membernames,
            backpack,
            ctime,
            mtime
        ) VALUES(
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?
        )
        ";
            var fpath = cw.util.relpath(header.fpath, this.ypath);
            fpath = cw.util.join_paths(fpath);
            var ctime = time.time();
            var mtime = os.path.getmtime(header.fpath);
            this.cur.execute(s, Tuple.Create(fpath, header.name, header.money, "\n".join(header.members), "\n".join(header.membernames), "\n".join(header.backpack), ctime, mtime));
            if (commit) {
                this.con.commit();
            }
        }
        
        [synclock(_lock)]
        public virtual object insert_partyrecord(object path, object commit = true) {
            return this._insert_partyrecord(path, commit);
        }
        
        public virtual object _insert_partyrecord(object path, object commit = true) {
            try {
                var header = cw.header.PartyRecordHeader(fpath: path);
                return this._insert_partyrecordheader(header, commit);
            } catch (Exception) {
                cw.util.print_ex();
            }
        }
        
        public virtual object get_partyrecord() {
            var s = "SELECT * FROM partyrecord ORDER BY name";
            this.cur.execute(s);
            var headers = new List<object>();
            foreach (var rec in this.cur) {
                var header = cw.header.PartyRecordHeader(dbrec: rec);
                header.fpath = cw.util.join_paths(this.ypath, header.fpath);
                headers.append(header);
            }
            return headers;
        }
        
        [synclock(_lock)]
        public virtual object insert_savedjpdcimageheader(object header, object commit = true) {
            return this._insert_savedjpdcimageheader(header, commit);
        }
        
        // データベースに保存されたJPDCイメージの情報を登録する。
        public virtual object _insert_savedjpdcimageheader(object header, object commit = true) {
            var s = @"
        INSERT OR REPLACE INTO savedjpdcimage(
            fpath,
            scenarioname,
            scenarioauthor,
            dpath,
            fpaths,
            ctime,
            mtime
        ) VALUES(
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?
        )
        ";
            var fpath = cw.util.relpath(header.fpath, this.ypath);
            fpath = cw.util.join_paths(fpath);
            var ctime = time.time();
            var mtime = os.path.getmtime(header.fpath);
            this.cur.execute(s, Tuple.Create(fpath, header.scenarioname, header.scenarioauthor, header.dpath, "\n".join(header.fpaths), ctime, mtime));
            if (commit) {
                this.con.commit();
            }
        }
        
        [synclock(_lock)]
        public virtual object insert_savedjpdcimage(object path, object commit = true) {
            return this._insert_savedjpdcimage(path, commit);
        }
        
        public virtual object _insert_savedjpdcimage(object path, object commit = true) {
            try {
                var header = cw.header.SavedJPDCImageHeader(fpath: path);
                return this._insert_savedjpdcimageheader(header, commit);
            } catch (Exception) {
                cw.util.print_ex();
            }
        }
        
        public virtual object get_savedjpdcimage() {
            var s = "SELECT * FROM savedjpdcimage";
            this.cur.execute(s);
            var d = new Dictionary<object, object> {
            };
            foreach (var rec in this.cur) {
                var header = cw.header.SavedJPDCImageHeader(dbrec: rec);
                header.fpath = cw.util.join_paths(this.ypath, header.fpath);
                d[Tuple.Create(header.scenarioname, header.scenarioauthor)] = header;
            }
            return d;
        }
        
        [synclock(_lock)]
        public virtual object commit() {
            this.con.commit();
        }
        
        [synclock(_lock)]
        public virtual object close() {
            this.con.close();
        }
    }
}
