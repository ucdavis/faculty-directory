import React, {  } from 'react';
import { useParams } from 'react-router-dom';

export const Person = () => {
  let { id } = useParams();

  return (
    <div>
        Person {id}
    </div>
  );
};
