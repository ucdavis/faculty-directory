import React, { useState } from 'react';
import { Button, Input, InputGroup, InputGroupAddon } from 'reactstrap';

export const UserInput = (props: any) => {
    const [username, setUsername] = useState("");

    const handleUsername = (e: any) => {
        setUsername(e.target.value)
    }

    const onSubmit = async (e: any) => {
        e.preventDefault();

        const headers = {
            Accept: 'application/json',
            'Content-Type': 'application/json'
        };

        const userData = JSON.stringify({ "username": username });

        const response = await fetch('api/users', {
            method: 'POST',
            headers,
            body: userData
        });

        const newUser = response.json();

        newUser.then((result) => {
            props.onChange([...props.users, result]);
        })
    };

    return (
        <InputGroup>
            <Input placeholder="Add a user" value={username} onChange={handleUsername} />
            <InputGroupAddon addonType="append"><Button color="primary" onClick={onSubmit}>Submit</Button></InputGroupAddon>
        </InputGroup>
    );
};