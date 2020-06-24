import React from 'react';
import { Table, Button } from 'reactstrap';

export const Users = () => {
    return (
        <Table>
            <tbody>
                <tr>
                    <th scope="row">1</th>
                    <td>Mark</td>
                    <td>Otto</td>
                    <Button color="danger">danger</Button>{' '}
                </tr>
            </tbody>
        </Table>
    );
};
