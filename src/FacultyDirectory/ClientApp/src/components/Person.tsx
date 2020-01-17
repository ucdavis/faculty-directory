import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';

export const Person = () => {
  let { id } = useParams();

  const [person, setPerson] = useState<any>(null);
  const [sitePerson, setSitePerson] = useState<any>({});

  useEffect(() => {
    const fetchPerson = async () => {
      const result = await fetch('SitePeople/' + id).then(r => r.json());

      setPerson(result.person);
      setSitePerson(result.sitePerson || {});
    };

    fetchPerson();
  }, [id]);

  const onSubmit = async (e: any) => {
    e.preventDefault();
    console.log('submitting', sitePerson);

    const headers = {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    };
    const body = JSON.stringify(sitePerson);
    const result = await fetch('SitePeople/' + id, { method: 'POST', headers, body }).then(r => r.json());

    console.log('api result', result);

  };

  const changeHandler = (event: any) => {
    const name = event.target.name;
    const value = event.target.value;

    setSitePerson({
      ...sitePerson,
      [name]: value
    });
  };

  if (!person) {
    return <div>loading</div>;
  }

  const hasSitePerson = !!sitePerson.id;

  return (
    <div>
      <h2>
        Person {person.firstName} {person.lastName}
      </h2>
      <form onSubmit={onSubmit}>
        <div className='form-group'>
          <label>First Name</label>
          <input
            type='text'
            className='form-control'
            name='firstName'
            placeholder={person.firstName}
            value={sitePerson.firstName || ''}
            onChange={changeHandler}
          />
        </div>
        <div className='form-group'>
          <label>Last Name</label>
          <input
            type='text'
            className='form-control'
            name='lastName'
            placeholder={person.lastName}
            value={sitePerson.lastName || ''}
            onChange={changeHandler}
          />
        </div>
        <div className='form-group'>
          <label>Title</label>
          <input
            type='text'
            className='form-control'
            name='title'
            placeholder={person.title}
            value={sitePerson.title || ''}
            onChange={changeHandler}
          />
        </div>
        {hasSitePerson && (
          <button type='submit' className='btn btn-primary'>
            Save Changes
          </button>
        )}
        {!hasSitePerson && (
          <button type='submit' className='btn btn-primary'>
            Save and Sync
          </button>
        )}
      </form>
      <div></div>
    </div>
  );
};
