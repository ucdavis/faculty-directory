import React from 'react';

interface IProps {
  hasActivity: boolean;
  children: any;
}

export const ActivityWrapper = (props: IProps) => {
  let css = '';

  if (props.hasActivity) {
    css = 'custom-active-wrapper';
  }

  return <div className={css}>{props.children}</div>;
};
