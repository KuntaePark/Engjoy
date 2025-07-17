const mysql = require('mysql2/promise');


class UserDataDB {
    constructor() {
        this.DBConnection = null;
        mysql.createConnection({
            host : '210.219.170.244',
            port: '3306',
            user: 'abc',
            password: '1234',
            database: 'engjoy'
        }).then((connection) => {
            console.log('UserDataDB connected');
            this.DBConnection = connection;
        }).catch((err) => {
            console.error('Error connecting to UserDataDB:', err);
        });
    }

    getUserData(userId) {
        if (!this.DBConnection) {
            throw new Error('Database connection not established');
        }

        //쿼리 실행
        return this.DBConnection.query("select * " +
                                        "from account a left join user_game_data u " + 
                                        "on a.account_id = u.account_id where a.account_id= ?", [userId])
        .then(([rows]) => {
            if (rows.length > 0) {
                return rows[0];
            } else {
                throw new Error('User not found');
            }
        });
    }

    saveGameResult(updatedPoint, userId) {
        if(!this.DBConnection) {
            throw new Error('Database connection not established.');
            //쿼리 실행
        }
        this.DBConnection.query("update user_game_data set game1score = ? where account_id = ?", [updatedPoint, userId])
        .then(([result]) => {
            if (result.changedRows != 1) {
                //고유 아이디이므로 1이여야 함
                throw new Error('User not found');
            }
        });  
    }

    saveUsedExpressions(userId, exprIds) {
        if(exprIds.length === 0) return; //사용한 단어 없이 진 경우

        //사용 단어 저장
        const placeholders = exprIds.map(() => '(?, ?, ?)').join(',');
        const sql = `
            insert into expr_used (account_id, expr_id, used_time)
            values ${placeholders}
            on duplicate key update
            used_time = values(used_time)
        `;
        const values = exprIds.flatMap(exprId => [userId, exprId, new Date()]);
        this.DBConnection.query(sql, values)
        .then(([result]) => {
            console.log(result);
        });
    }

    saveGame2Result(game2score, gold, userId) {
        if(!this.DBConnection) {
            throw new Error(`Database connection not established.`);
        }
        this.DBConnection.query("update user_game_data set game2high_score = ?, gold = ? where account_id = ?", [game2score, gold, userId])
        .then(([result]) => {
            if (result.changedRows != 1) {
                throw new Error('User not found');
            }
        });  
    }
}

module.exports = UserDataDB;