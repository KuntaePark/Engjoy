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
        return this.DBConnection.query("select a.nickname, u.game1score, u.body_type_index, u.weapon_type_index " +
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

    updateUserPoint(updatedPoint, userId) {
        if(!this.DBConnection) {
            throw new Error('Database connection not established.');
            //쿼리 실행
        }
        return this.DBConnection.query("update user_game_data set game1score = ? where account_id = ?", [updatedPoint, userId])
        .then(([result]) => {
            if (result.changedRows == 1) {
                //고유 아이디이므로 1이여야 함
                return 1;
            } else {
                throw new Error('User not found');
            }
        });
    }

    updateGame2Data(gameState, userId) {
        if(!this.DBConnection) {
            throw new Error(`Database connection not established.`);
        }
    //해당 문장 키값을 기반으로 유저의 사용 문장 업데이트
    }
}

module.exports = UserDataDB;