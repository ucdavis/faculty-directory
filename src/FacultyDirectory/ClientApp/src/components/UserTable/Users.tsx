import React, { useState, useEffect } from 'react';
import { Table, Button } from 'reactstrap';
import { IUser } from '../../models/IUsers';
import { UserInput } from './UserInput';

export const Users = () => {
    const [users, setUsers] = useState<IUser[]>([]);

    useEffect(() => {
        const getUsers = async () => {
            const results = await fetch('api/users').then(r => r.json());
            setUsers(results);
        };
        getUsers();
    }, []);

    const onSubmit = async (e: any, id: number) => {
        e.preventDefault();

        const headers = {
            Accept: 'application/json',
            'Content-Type': 'application/json'
        };

        await fetch('api/users/' + id, {
            method: 'DELETE',
            headers,
        });
        
        setUsers(users.filter(user => user.id !== id));
    };

    return (
        <div>
            <UserInput users={users} onChange={setUsers} />

            <Table>
                <tbody>
                    {users?.map((user) => (
                        <tr key={user.id}>
                            <td>{user.username}</td>
                            <td>
                                {/* <Button color="danger" onClick={() => { console.log("hi") }}>danger</Button>{' '} */}
                                <Button color="danger" onClick={e => { onSubmit(e, user.id) }}>danger</Button>{' '}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </div>
    );
};
