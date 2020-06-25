import React, { useState, useEffect } from 'react';
import { Table } from 'reactstrap';
import { IUser } from '../../models/IUsers';
import { UserInput } from './UserInput';

export const Users = () => {
    const [users, setUsers] = useState<IUser[]>([]);

    useEffect(() => {
        const getUsers = async () => {
            const results = await fetch('api/users/all').then(r => r.json());
            setUsers(results);
        };
        getUsers();
    }, []);

    return (
        <div>
            <UserInput users={users} onChange={setUsers} />

            <Table>
                <tbody>
                    {users?.map((user) => (
                        <tr key={user.id}>
                            <td>{user.username}</td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </div>
    );
};
